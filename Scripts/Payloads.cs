using LiteNetLib.Utils;

public struct InputPayload : INetSerializable
{
    public int tick;
    public float xAxis;
    public float yAxis;
    // In the case of network uncertainty, this will be the last flip tick.
    // If local input, then 0 is no input, 1 is input.
    public int flip;

    public void Deserialize(NetDataReader reader)
    {
        tick = reader.GetInt();
        xAxis = reader.GetFloat();
        yAxis = reader.GetFloat();
        flip = reader.GetInt();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(tick);
        writer.Put(xAxis);
        writer.Put(yAxis);
        writer.Put(flip);
    }
}

public struct StatePayload : INetSerializable
{
    public int tick;
    public CarState car1;
    public CarState car2;
    public BallState ball;

    public void Deserialize(NetDataReader reader)
    {
        tick = reader.GetInt();
        car1.Deserialize(reader);
        car2.Deserialize(reader);
        ball.Deserialize(reader);
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(tick);
        car1.Serialize(writer);
        car2.Serialize(writer);
        ball.Serialize(writer);
    }
}

public struct CarState : INetSerializable
{
    public float xPosition;
    public float yPosition;

    public float xVelocity;
    public float yVelocity;

    public float rotation;

    public int flipState;
    public bool canFlip;

    public void Deserialize(NetDataReader reader)
    {
        xPosition = reader.GetFloat();
        yPosition = reader.GetFloat();

        xVelocity = reader.GetFloat();
        yVelocity = reader.GetFloat();

        rotation = reader.GetFloat();

        flipState = reader.GetInt();
        canFlip = reader.GetBool();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(xPosition);
        writer.Put(yPosition);

        writer.Put(xVelocity);
        writer.Put(yVelocity);

        writer.Put(rotation);

        writer.Put(flipState);
        writer.Put(canFlip);
    }
}

public struct BallState : INetSerializable
{
    public float xPosition;
    public float yPosition;

    public float xVelocity;
    public float yVelocity;

    public float rotation;
    public float rotationVelocity;

    public void Deserialize(NetDataReader reader)
    {
        xPosition = reader.GetFloat();
        yPosition = reader.GetFloat();

        xVelocity = reader.GetFloat();
        yVelocity = reader.GetFloat();

        rotation = reader.GetFloat();
        rotationVelocity = reader.GetFloat();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(xPosition);
        writer.Put(yPosition);

        writer.Put(xVelocity);
        writer.Put(yVelocity);

        writer.Put(rotation);
        writer.Put(rotationVelocity);
    }
}
