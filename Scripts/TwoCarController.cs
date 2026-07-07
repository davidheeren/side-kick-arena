using System.Net;
using System.Net.Sockets;
using Godot;
using LiteNetLib;
using LiteNetLib.Utils;

public partial class TwoCarController : Node, INetEventListener
{
    [Export] Car car1;
    [Export] Car car2;
    [Export] Ball ball;

    int port = 9050;
    string connectionKey = "supersecret";
    float peerPitchScale = 0.75f;

    bool isRunning = false;
    bool isHost = false;

    NetManager manager;
    // Peer is client if this is the host, and vice versa
    NetPeer netPeer;

    Rid space;
    float physicsDelta; // Get from settings
    ulong networkDelta = 1000 / 30; // 30 hz
    ulong lastNetworkTime = 0;
    ulong nTick = 0;

    ulong clientLastFlipTick = 0;
    bool clientShouldFlip = false;
    Vector2 clientLastMove = Vector2.Zero;

    StatePayload? lastState;

    public override void _Ready()
    {
        Debug.AssertNotNull(car1, "Car 1 cannot be null");
        Debug.AssertNotNull(car2, "Car 2 cannot be null");
        Debug.AssertNotNull(ball, "Ball cannot be null");

        physicsDelta = 1 / (float)ProjectSettings.GetSetting("physics/common/physics_ticks_per_second");
        space = GetViewport().GetWorld2D().Space;
        PhysicsServer2D.SetActive(false);
    }

    public override void _Process(double delta)
    {
        if (!isRunning)
            return;

        manager.PollEvents();

        if (Time.GetTicksMsec() - lastNetworkTime > networkDelta)
        {
            lastNetworkTime = Time.GetTicksMsec();
            nTick++;

            NetworkTick();
        }
    }

    private void NetworkTick()
    {
        if (netPeer == null)
            return;

        if (isHost)
        {
            StatePayload statePayload = Simulator.GetState(car1, car2, ball, nTick);
            NetDataWriter writer = new NetDataWriter();
            writer.Put(statePayload);
            netPeer.Send(writer, DeliveryMethod.Unreliable);
        }
        else
        {
            InputPayload inputPayload = new InputPayload
            {
                tick = nTick,
                moveX = clientLastMove.X,
                moveY = clientLastMove.Y,
                flip = clientLastFlipTick,
            };

            NetDataWriter writer = new NetDataWriter();
            writer.Put(inputPayload);
            netPeer.Send(writer, DeliveryMethod.Unreliable);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!isRunning)
            return;

        if (isHost)
        {
            // Car 1
            Vector2 hostMoveInput = Car.LimitMoveInput(Car.GetMoveInput());
            car1.MoveHorizontal(hostMoveInput.X);
            car1.MoveVertical(hostMoveInput.Y);
            if (Input.IsActionJustPressed("flip"))
                car1.Flip(hostMoveInput.Normalized());
            car1.UpdateCanFlip();

            // Car 2
            car2.MoveHorizontal(clientLastMove.X);
            car2.MoveVertical(clientLastMove.Y);
            if (clientShouldFlip)
            {
                car2.Flip(clientLastMove.Normalized());
                clientShouldFlip = false;
            }
            car2.UpdateCanFlip();
        }
        else
        {
            clientLastMove = Car.LimitMoveInput(Car.GetMoveInput());
            // car2.MoveHorizontal(clientLastMove.X);
            // car2.MoveVertical(clientLastMove.Y);

            if (Input.IsActionJustPressed("flip"))
            {
                // car2.Flip(clientLastMove.Normalized());
                clientLastFlipTick = nTick;
            }

            // car2.UpdateCanFlip();

            // car1.MoveHorizontal(0);
            // car1.MoveVertical(0);

            if (lastState != null)
            {
                Simulator.SetState(car1, car2, ball, lastState.Value);
                lastState = null;
            }
        }
    }

    public void StartHost()
    {
        PhysicsServer2D.SetActive(true);
        isRunning = true;
        isHost = true;

        manager = new NetManager(this);
        manager.Start(port);
        car2.GetNode<AudioStreamPlayer2D>("GetFlipSFX").PitchScale = peerPitchScale;
        car2.GetNode<AudioStreamPlayer2D>("FlipSFX").PitchScale = peerPitchScale;
    }

    public void StartClient(string ipAddress)
    {
        PhysicsServer2D.SetActive(true);
        isRunning = true;

        manager = new NetManager(this);
        manager.Start();
        manager.Connect(ipAddress, port, connectionKey);
        car1.GetNode<AudioStreamPlayer2D>("GetFlipSFX").PitchScale = peerPitchScale;
        car1.GetNode<AudioStreamPlayer2D>("FlipSFX").PitchScale = peerPitchScale;
    }


    public void OnPeerConnected(NetPeer peer)
    {
        netPeer = peer;
        if (isHost)
            GD.Print($"Client connected: {netPeer}");
        else
            GD.Print($"Connected to host: {netPeer}");
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) { }
    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError) { }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        if (isHost)
        {
            InputPayload inputPayload = reader.Get<InputPayload>();
            if (inputPayload.flip > clientLastFlipTick)
                clientShouldFlip = true;
            clientLastFlipTick = inputPayload.flip;
            clientLastMove = Car.LimitMoveInput(new Vector2(inputPayload.moveX, inputPayload.moveY));
        }
        else
        {
            StatePayload statePayload = reader.Get<StatePayload>();
            lastState = statePayload;
            // Simulator.SetState(car1, car2, ball, statePayload);
        }
        reader.Recycle();
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }
    public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        if (manager.ConnectedPeersCount < 1)
            request.AcceptIfKey(connectionKey);
        else
            request.Reject();
    }
}
