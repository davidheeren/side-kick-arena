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

    public override void _Ready()
    {
        Debug.AssertNotNull(car1, "Car 1 cannot be null");
        Debug.AssertNotNull(car2, "Car 2 cannot be null");
        Debug.AssertNotNull(ball, "Ball cannot be null");

        physicsDelta = 1 / (float)ProjectSettings.GetSetting("physics/common/physics_ticks_per_second");
        space = GetViewport().GetWorld2D().Space;
        PhysicsServer2D.SetActive(false);
    }

    private Vector2 GetMoveInput()
    {
        // Get non normalized input
        // Y is flipped
        Vector2 input;
        input.X = Input.GetAxis("move_left", "move_right");
        input.Y = Input.GetAxis("move_up", "move_down");
        return input;
    }

    private Vector2 LimitMoveInput(Vector2 input)
    {
        float overDeadzone = 0.8f;
        // Set input to max if abs over
        if (input.X > overDeadzone)
            input.X = 1;
        else if (input.X < -overDeadzone)
            input.X = -1;
        if (input.Y > overDeadzone)
            input.Y = 1;
        else if (input.Y < -overDeadzone)
            input.Y = -1;
        return input;
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
            Vector2 hostMoveInput = LimitMoveInput(GetMoveInput());
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
            clientLastMove = LimitMoveInput(GetMoveInput());

            if (Input.IsActionJustPressed("flip"))
                clientLastFlipTick = nTick;
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
        // PhysicsServer2D.SetActive(true);
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
            clientLastMove = LimitMoveInput(new Vector2(inputPayload.moveX, inputPayload.moveY));
        }
        else
        {
            StatePayload statePayload = reader.Get<StatePayload>();
            Simulator.SetState(car1, car2, ball, statePayload);
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
