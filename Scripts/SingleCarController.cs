using Godot;

public partial class SingleCarController : Node
{
    [Export] Car car;

    public void Start()
    {
        PhysicsServer2D.SetActive(true);
    }

    public override void _PhysicsProcess(double delta)
    {
        GD.PrintErr("Should not be using single car controller yet");
        Vector2 hostMoveInput = Car.LimitMoveInput(Car.GetMoveInput());
        car.MoveHorizontal(hostMoveInput.X);
        car.MoveVertical(hostMoveInput.Y);
        if (Input.IsActionJustPressed("flip"))
            car.Flip(hostMoveInput.Normalized());
        car.UpdateCanFlip();
    }
}
