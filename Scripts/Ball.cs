using Godot;

public partial class Ball : RigidBody2D
{
    public const float ppu = 16;

    [Export] float maxSpeed = 6;
    [Export] float overDecay = 0.05f;
    [Export] float alwaysDecay = 0.005f;

    public override void _Process(double delta)
    {
        Vector2 force = -LinearVelocity * alwaysDecay * ppu;

        if (LinearVelocity.Length() > maxSpeed * ppu)
        {
            // GD.Print("Over");
            force += -LinearVelocity * overDecay * ppu;
        }

        ApplyCentralForce(force * Mass);
    }
}
