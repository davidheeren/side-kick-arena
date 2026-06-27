using Godot;
using Godot.Collections;

public partial class Car : RigidBody2D
{
    public const float ppu = 16;

    [Export] float flipSpeed = 8;
    [Export] float jumpSpeed = 4;

    [Export] float verticalForce = 20;
    [Export] float verticalDecay = 1;

    [Export] float moveSpeed = 10;
    [Export] float moveDecay = 5;
    [Export] float stopDecay = 3;
    [Export] float overMoveDecay = 0.5f;

    [Export] float rotationSpeed = 640;

    [Export] public Sprite2D sprite { get; private set; }
    [Export] AudioStreamPlayer2D getFlipAudio;
    [Export] AudioStreamPlayer2D flipAudio;

    Rid space;
    float fixedDelta;

    // Not flipping is 0
    // 1: positive rotation, 2: negative
    public int flipState;
    public bool canFlip = true;

    public override void _Ready()
    {
        space = GetWorld2D().Space;
        fixedDelta = 1 / (float)ProjectSettings.GetSetting("physics/common/physics_ticks_per_second");
    }

    public void MoveHorizontal(float input)
    {
        // Add force with exponential decay towards target speed
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

        // Flip sprite
        if (input > 0 && sprite.FlipH)
            sprite.FlipH = false;
        else if (input < 0 && !sprite.FlipH)
            sprite.FlipH = true;
    }

    public void MoveVertical(float input)
    {
        // Jump to speed if on the ground and not already speed
        bool grounded = Raycast(Vector2.Down * 9);
        float velDiff = jumpSpeed * ppu + LinearVelocity.Y;
        if (grounded && input < 0 && velDiff > 0)
            ApplyCentralImpulse(Vector2.Up * velDiff * Mass);

        // Constant force
        float force = input * verticalForce * ppu;

        // Add tiny amount of control if not overspeeding
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
        // Exit if can't flip or already flipping
        if (!canFlip || flipState != 0)
            return;
        canFlip = false;

        // Default to up if no direction
        if (input == Vector2.Zero)
            input = Vector2.Up;

        Vector2 flipImpulse = input * flipSpeed * ppu;

        // Set not moving in flip direction, cancel out negative velocity
        if (input.X != 0 && Mathf.Sign(input.X) != Mathf.Sign(LinearVelocity.X))
            flipImpulse.X -= LinearVelocity.X;
        if (input.Y != 0 && Mathf.Sign(input.Y) != Mathf.Sign(LinearVelocity.Y))
            flipImpulse.Y -= LinearVelocity.Y;

        ApplyCentralImpulse(flipImpulse * Mass);

        // Set flip direction depending on which the car is facing
        flipState = sprite.FlipH ? -1 : 1;

        flipAudio.Play();
    }

    public void UpdateCanFlip()
    {
        // If not flipping or after degrees rotated after flipping
        //  then allow flip
        if (flipState == 0 || Mathf.Abs(sprite.RotationDegrees) >= 90)
        {
            Vector2 rayDir = Vector2.Down.Rotated(sprite.Rotation);
            if (Raycast(rayDir * ppu) && !canFlip)
            {
                canFlip = true;
                getFlipAudio.Play();
            }
        }
    }

    public override void _PhysicsProcess(double delta) { }

    public void UpdateFlipAnimation(double delta)
    {
        // Exit if not flipping
        if (flipState == 0)
            return;

        // If going to rotate over 360, then stop flipping
        //  otherwise rotate some amount
        float offsetDeg = flipState * rotationSpeed * (float)delta;
        float possibleDeg = sprite.RotationDegrees + offsetDeg;
        if (possibleDeg > 360 || possibleDeg < -360)
        {
            flipState = 0;
            sprite.RotationDegrees = 0;
        }
        else
        {
            sprite.RotationDegrees = possibleDeg;
        }
    }

    public override void _Process(double delta)
    {
        UpdateFlipAnimation(delta);

        // Example of how to step throught the simulation
        // if (Input.IsActionJustPressed("flip"))
        // {
        //     // PhysicsServer2D.SetActive(false);
        //     for (int i = 0; i < 30; i++)
        //     {
        //         PhysicsServer2D.Singleton.Call("space_step", space, fixedDelta);
        //     }
        //     PhysicsServer2D.Singleton.Call("space_flush_queries", space);
        // }
    }

    public Vector2 GetInput()
    {
        // Get non normalized input
        // Y is flipped
        float overDeadzone = 0.8f;
        Vector2 input;

        input.X = Input.GetAxis("move_left", "move_right");
        input.Y = Input.GetAxis("move_up", "move_down");

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

    public bool Raycast(Vector2 dir)
    {
        // https://docs.godotengine.org/en/stable/tutorials/physics/ray-casting.html
        PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;
        PhysicsRayQueryParameters2D query = PhysicsRayQueryParameters2D.Create(Position, Position + dir);
        Dictionary result = spaceState.IntersectRay(query);
        return result.Count > 0;
    }
}
