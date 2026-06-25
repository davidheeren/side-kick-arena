
public struct InputPayload
{
    public int tick;
    public float xAxis;
    public float yAxis;
    // In the case of of network uncertanty, this will be the last flip tick
    // If local input, then 0 is no input, 1 is input
    public int flip;
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
