
public struct InputPayload
{
    public int tick;
    public float xAxis;
    public float yAxis;
    public int lastFlipTick;
}

public struct StatePayload
{
    public int tick;
    public CarState car1;
    public CarState car2;
    public BallState ball;
}

public struct CarState
{
    public float xPosition;
    public float yPosition;

    public float xVelocity;
    public float yVelocity;

    public float rotation;

    public int flipState;
    public bool canFlip;
}

public struct BallState
{
    public float xPosition;
    public float yPosition;

    public float xVelocity;
    public float yVelocity;

    public float rotation;
    public float rotationVelocity;
}
