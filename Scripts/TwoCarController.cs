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

    bool isRunning = false;
    bool isHost = false;

    NetManager manager;
    NetPeer netPeer;

    public override void _Ready()
    {
        PhysicsServer2D.SetActive(false);
    }

    public override void _Process(double delta)
    {
        if (!isRunning)
            return;

        manager.PollEvents();

        if (isHost && netPeer != null)
        {
            StatePayload statePayload = GetState();
            NetDataWriter writer = new NetDataWriter();
            writer.Put(statePayload);
            netPeer.Send(writer, DeliveryMethod.Unreliable);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!isRunning || !isHost)
            return;

        Vector2 input = car1.GetInput();
        car1.MoveHorizontal(input.X);
        car1.MoveVertical(input.Y);

        // Flip dir normalized
        if (Input.IsActionJustPressed("flip"))
            car1.Flip(input.Normalized());

        car1.UpdateCanFlip();
    }

    public void StartHost()
    {
        PhysicsServer2D.SetActive(true);
        isRunning = true;
        isHost = true;

        manager = new NetManager(this);
        manager.Start(port);
    }

    public void StartClient(string ipAddress, int port)
    {
        // PhysicsServer2D.SetActive(true);
        isRunning = true;

        manager = new NetManager(this);
        manager.Start();
        manager.Connect(ipAddress, port, connectionKey);
    }

    public StatePayload GetState()
    {
        CarState carState1 = new CarState
        {
            xPosition = car1.GlobalPosition.X,
            yPosition = car1.GlobalPosition.Y,
            xVelocity = car1.LinearVelocity.X,
            yVelocity = car1.LinearVelocity.Y,
            rotation = car1.sprite.RotationDegrees,
            flipState = car1.flipState,
            canFlip = car1.canFlip,
        };

        BallState ballState = new BallState
        {
            xPosition = ball.GlobalPosition.X,
            yPosition = ball.GlobalPosition.Y,
            xVelocity = ball.LinearVelocity.X,
            yVelocity = ball.LinearVelocity.Y,
            rotation = ball.GlobalRotationDegrees,
            rotationVelocity = ball.AngularVelocity,
        };

        return new StatePayload
        {
            car1 = carState1,
            ball = ballState,
        };
    }

    public void SetState(StatePayload state)
    {
        // Car 1
        car1.GlobalPosition = new Vector2(state.car1.xPosition, state.car1.yPosition);
        // car1.LinearVelocity = new Vector2(state.car1.xVelocity, state.car1.yVelocity);
        car1.sprite.RotationDegrees = state.car1.rotation;
        car1.flipState = state.car1.flipState;
        car1.canFlip = state.car1.canFlip;

        // Ball
        ball.GlobalPosition = new Vector2(state.ball.xPosition, state.ball.yPosition);
        // ball.LinearVelocity = new Vector2(state.ball.xVelocity, state.ball.yVelocity);
        ball.GlobalRotationDegrees = state.ball.rotation;
        // ball.AngularVelocity = state.ball.rotationVelocity;
    }

    public void OnPeerConnected(NetPeer peer)
    {
        netPeer = peer;
        if (isHost)
        {
            GD.Print($"Client connected: {netPeer}");
        }
        else
        {
            GD.Print($"Connected to host: {netPeer}");
        }
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) { }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError) { }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        if (isHost) { }
        else
        {
            StatePayload statePayload = reader.Get<StatePayload>();
            SetState(statePayload);
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
