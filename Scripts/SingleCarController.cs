using Godot;

public partial class SingleCarController : Node
{
    [Export] Car car;

    public override void _PhysicsProcess(double delta)
    {
        Vector2 input = car.GetInput();
        car.MoveHorizontal(input.X);
        car.MoveVertical(input.Y);

        // Flip dir normalized
        if (Input.IsActionJustPressed("flip"))
            car.Flip(input.Normalized());

        car.UpdateCanFlip();
    }
}
