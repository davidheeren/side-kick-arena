using Godot;

public partial class Car : RigidBody2D
{
    // Assumes mass of 1

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

        // Overspeeding
        if (LinearVelocity.X != 0 && (LinearVelocity.X - speed) * Mathf.Sign(speed) > 0)
            force = (speed - LinearVelocity.X) * overMoveDecay;
        // Stopping
        else if (input == 0)
            force = (speed - LinearVelocity.X) * stopDecay;
        // Moving
        else
            force = (speed - LinearVelocity.X) * moveDecay;

        ApplyCentralForce(Vector2.Right * force);
    }

    public void MoveVertical(float input)
    {
        float force = input * verticalForce * ppu;

        // Maybe apply a base pure force with a
        // tiny addition of smoothing like for horizontal

        ApplyCentralForce(Vector2.Down * force);
    }

    public void Flip(Vector2 input)
    {
        if (input == Vector2.Zero)
            input = Vector2.Up;

        Vector2 flipImpulse = input * flipSpeed * ppu;

        // Set not moving in flip direction, cancel out negative velocity
        if (Mathf.Sign(input.X) != Mathf.Sign(LinearVelocity.X))
            flipImpulse.X -= LinearVelocity.X;
        if (Mathf.Sign(input.Y) != Mathf.Sign(LinearVelocity.Y))
            flipImpulse.Y -= LinearVelocity.Y;

        ApplyCentralImpulse(flipImpulse);
    }


    public override void _PhysicsProcess(double delta)
    {
        float overDeadzone = 0.8f;
        Vector2 input = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        // I don't think normalizing the dir works the best for regular movement
        // I think its better to have each axis operate independantlly
        // if (input.Length() > overDeadzone)
        // input = input.Normalized();

        // Over dead zone
        if (input.X > overDeadzone)
            input.X = 1;
        else if (input.X < -overDeadzone)
            input.X = -1;
        if (input.Y > overDeadzone)
            input.Y = 1;
        else if (input.Y < -overDeadzone)
            input.Y = -1;

        MoveHorizontal(input.X);
        MoveVertical(input.Y);

        // Flip dir normalized
        if (Input.IsActionJustPressed("flip"))
            Flip(input.Normalized());
    }
}
