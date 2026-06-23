using Godot;

public partial class Car : RigidBody2D
{
    public const float ppu = 16;
    [Export] float moveSpeed = 10;
    [Export] float verticalForce = 20;
    [Export] float flipSpeed = 8;
    [Export] float moveDecay = 5;
    [Export] float stopDecay = 3;
    [Export] float overMoveDecay = 0.5f;

    public void MoveHorizontal(float input)
    {
        float speed = input * moveSpeed * ppu;
        float force;

        if (LinearVelocity.X != 0 && (LinearVelocity.X - speed) * Mathf.Sign(speed) > 0)
            force = (speed - LinearVelocity.X) * overMoveDecay;
        else if (input == 0)
            force = (speed - LinearVelocity.X) * stopDecay;
        else
            force = (speed - LinearVelocity.X) * moveDecay;

        ApplyCentralForce(Vector2.Right * force);
    }

    public void MoveVertical(float input)
    {
        float force = input * verticalForce * ppu;

        ApplyCentralForce(Vector2.Down * force);
    }

    public void Flip(Vector2 input)
    {
        if (input == Vector2.Zero)
            input = Vector2.Up;

        // TODO: If velocity < 0, then set velocity to 0 and impulse

        ApplyCentralImpulse(input.Normalized() * flipSpeed * ppu);
    }


    public override void _PhysicsProcess(double delta)
    {
        float overDeadzone = 0.8f;
        Vector2 input = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        // I don't think normalizing the dir works the best for regular movement
        // if (input.Length() > overDeadzone)
        // input = input.Normalized();

        if (input.X > overDeadzone)
            input.X = 1;
        if (input.Y > overDeadzone)
            input.Y = 1;

        MoveHorizontal(input.X);
        MoveVertical(input.Y);

        if (Input.IsActionJustPressed("flip"))
            Flip(input);
    }
}
