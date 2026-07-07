using Godot;

public partial class Ball : RigidBody2D, IEntity
{
    public const float ppu = 16;

    [Export] float maxSpeed = 6;
    [Export] float overDecay = 0.05f;
    [Export] float alwaysDecay = 0.005f;

    public Vector2? targetVelocity;
    public Vector2 EVelocity { get => LinearVelocity; set => targetVelocity = value; }
    public Vector2 EPosition { get => GlobalPosition; set => GlobalPosition = value; }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 force = -LinearVelocity * alwaysDecay * ppu;

        if (LinearVelocity.Length() > maxSpeed * ppu)
        {
            // GD.Print("Over");
            force += -LinearVelocity * overDecay * ppu;
        }

        ApplyCentralForce(force * Mass);
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        if (targetVelocity == null)
            return;
        state.LinearVelocity = targetVelocity.Value;
        targetVelocity = null;
    }
}
