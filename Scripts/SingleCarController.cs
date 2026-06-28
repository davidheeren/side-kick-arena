using Godot;

public partial class SingleCarController : Node
{
    [Export] Car car;

    public override void _PhysicsProcess(double delta)
    {
        GD.PrintErr("Should not be using single car controller yet");
        // Vector2 input = ProcessMoveInput(GetMoveInput());
        Vector2 input = Vector2.Zero;
        car.MoveHorizontal(input.X);
        car.MoveVertical(input.Y);

        // Flip dir normalized
        if (Input.IsActionJustPressed("flip"))
            car.Flip(input.Normalized());

        car.UpdateCanFlip();
    }
}
