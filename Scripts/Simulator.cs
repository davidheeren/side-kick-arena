using System;
using System.Collections.Generic;
using Godot;

public static class Simulator
{
    public static void Simulate(Rid space, float fixedDelta, StatePayload startState, IEnumerable<InputPayload> inputs)
    {
        PhysicsServer2D.SetActive(false);
        for (int i = 0; i < 30; i++)
        {
            PhysicsServer2D.Singleton.Call("space_step", space, fixedDelta);
        }
        PhysicsServer2D.Singleton.Call("space_flush_queries", space);
        PhysicsServer2D.SetActive(true);
    }

    public static StatePayload GetState(Car car1, Car car2, Ball ball, ulong tick)
    {
        CarState carState1 = new CarState
        {
            xPosition = car1.GlobalPosition.X,
            yPosition = car1.GlobalPosition.Y,
            xVelocity = car1.LinearVelocity.X,
            yVelocity = car1.LinearVelocity.Y,
            rotation = car1.sprite.RotationDegrees,
            fliphH = car1.sprite.FlipH,
            flipState = car1.flipState,
            canFlip = car1.canFlip,
        };

        CarState carState2 = new CarState
        {
            xPosition = car2.GlobalPosition.X,
            yPosition = car2.GlobalPosition.Y,
            xVelocity = car2.LinearVelocity.X,
            yVelocity = car2.LinearVelocity.Y,
            rotation = car2.sprite.RotationDegrees,
            fliphH = car2.sprite.FlipH,
            flipState = car2.flipState,
            canFlip = car2.canFlip,
        };

        BallState ballState = new BallState
        {
            xPosition = ball.GlobalPosition.X,
            yPosition = ball.GlobalPosition.Y,
            xVelocity = ball.LinearVelocity.X,
            yVelocity = ball.LinearVelocity.Y,
            rotation = ball.GlobalRotationDegrees,
            rotationVelocity = ball.AngularVelocity,
        };

        return new StatePayload
        {
            tick = tick,
            car1 = carState1,
            car2 = carState2,
            ball = ballState,
        };
    }

    public static void SetState(Car car1, Car car2, Ball ball, StatePayload state)
    {
        // Car 1
        // car1.GlobalPosition = new Vector2(state.car1.xPosition, state.car1.yPosition);
        UpdatePosition(car1, new Vector2(state.car1.xPosition, state.car1.yPosition));
        // car1.LinearVelocity = new Vector2(state.car1.xVelocity, state.car1.yVelocity);
        UpdateVelocity(car1, new Vector2(state.car1.xVelocity, state.car1.yVelocity));
        car1.sprite.RotationDegrees = state.car1.rotation;
        car1.sprite.FlipH = state.car1.fliphH;
        car1.flipState = state.car1.flipState;
        car1.canFlip = state.car1.canFlip;

        // Car 2
        // car2.GlobalPosition = new Vector2(state.car2.xPosition, state.car2.yPosition);
        UpdatePosition(car2, new Vector2(state.car2.xPosition, state.car2.yPosition));
        // car2.LinearVelocity = new Vector2(state.car2.xVelocity, state.car2.yVelocity);
        UpdateVelocity(car2, new Vector2(state.car2.xVelocity, state.car2.yVelocity));
        car2.sprite.RotationDegrees = state.car2.rotation;
        car2.sprite.FlipH = state.car2.fliphH;
        car2.flipState = state.car2.flipState;
        car2.canFlip = state.car2.canFlip;

        // Ball
        ball.GlobalPosition = new Vector2(state.ball.xPosition, state.ball.yPosition);
        // UpdatePosition(ball, new Vector2(state.ball.xPosition, state.ball.yPosition));
        UpdateVelocity(ball, new Vector2(state.ball.xVelocity, state.ball.yVelocity));
        // ball.LinearVelocity = new Vector2(state.ball.xVelocity, state.ball.yVelocity);
        ball.GlobalRotationDegrees = state.ball.rotation;
        ball.AngularVelocity = state.ball.rotationVelocity;
    }

    private static void UpdatePosition(IEntity entity, Vector2 position)
    {
        Vector2 diff = position - entity.EPosition;
        float len = diff.Length();
        if (len > 8)
            entity.EPosition = position;
        else
            entity.EPosition += diff / 2;
    }

    private static void UpdateVelocity(IEntity entity, Vector2 velocity)
    {
        Vector2 diff = velocity - entity.EVelocity;
        float len = diff.Length();
        if (len > 8)
            entity.EVelocity = velocity;
        else
            entity.EVelocity += diff / 2;
    }
}
