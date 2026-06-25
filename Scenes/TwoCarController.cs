using Godot;

public partial class TwoCarController : Node
{
    [Export] Car car1;
    [Export] Car car2;
    [Export] Ball ball;

    public override void _PhysicsProcess(double delta)
    {
        Vector2 input = car1.GetInput();
        car1.MoveHorizontal(input.X);
        car1.MoveVertical(input.Y);

        // Flip dir normalized
        if (Input.IsActionJustPressed("flip"))
            car1.Flip(input.Normalized());

        car1.UpdateCanFlip();
    }
}
