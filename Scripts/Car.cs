using Godot;
using Godot.Collections;

public partial class Car : RigidBody2D
{
    // Assumes mass of 1

    public const float ppu = 16;

    [Export] float flipSpeed = 8;
    [Export] float jumpSpeed = 4;

    [Export] float verticalForce = 20;
    [Export] float verticalDecay = 1;

    [Export] float moveSpeed = 10;
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

        ApplyCentralForce(Vector2.Right * force * Mass);
    }

    public void MoveVertical(float input)
    {
        bool grounded = Raycast(Vector2.Down * ppu * 0.75f);
        float velDiff = jumpSpeed * ppu + LinearVelocity.Y;
        if (grounded && input < 0 && velDiff > 0)
        {
            ApplyCentralImpulse(Vector2.Up * velDiff * Mass);
            GD.Print("Jummped");
        }

        float force = input * verticalForce * ppu;

        // Tiny amount of control if not overspeeding
        if (input != 0)
        {
            float ajustForce = ((input * moveSpeed * ppu) - LinearVelocity.Y) * verticalDecay;
            if (Mathf.Sign(ajustForce) == Mathf.Sign(force))
                force += ajustForce;
        }

        ApplyCentralForce(Vector2.Down * force * Mass);
    }

    public void Flip(Vector2 input)
    {
        if (input == Vector2.Zero)
            input = Vector2.Up;

        Vector2 flipImpulse = input * flipSpeed * ppu;

        // Set not moving in flip direction, cancel out negative velocity
        if (input.X != 0 && Mathf.Sign(input.X) != Mathf.Sign(LinearVelocity.X))
        {
            flipImpulse.X -= LinearVelocity.X;
            GD.Print("X Impulse");
        }
        if (input.Y != 0 && Mathf.Sign(input.Y) != Mathf.Sign(LinearVelocity.Y))
        {
            flipImpulse.Y -= LinearVelocity.Y;
            GD.Print("Y Impulse:");
        }

        ApplyCentralImpulse(flipImpulse * Mass);
    }

    public override void _PhysicsProcess(double delta)
    {
        float overDeadzone = 0.8f;
        // Y is flipped
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

    public bool Raycast(Vector2 dir)
    {
        // https://docs.godotengine.org/en/stable/tutorials/physics/ray-casting.html
        PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;
        PhysicsRayQueryParameters2D query = PhysicsRayQueryParameters2D.Create(Position, Position + dir);
        Dictionary result = spaceState.IntersectRay(query);
        return result.Count > 0;
    }
}
