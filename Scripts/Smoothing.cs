using Godot;

public static class Smoothing
{
    /// <summary>
    /// Smooth Lerping
    /// </summary>
    /// <param name="a">Current Value</param>
    /// <param name="b">Target Value</param>
    /// <param name="decay">Value from 1 - 25, from slow to fast</param>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static float ExpDecay(float a, float b, float decay, float dt)
    {
        return b + (a - b) * Mathf.Exp(-decay * dt);
    }

    /// <summary>
    /// Smooth Lerping
    /// </summary>
    /// <param name="a">Current Value</param>
    /// <param name="b">Target Value</param>
    /// <param name="decay">Value from 1 - 25, from slow to fast</param>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static Vector2 ExpDecay(Vector2 a, Vector2 b, float decay, float dt)
    {
        return b + (a - b) * Mathf.Exp(-decay * dt);
    }

    /// <summary>
    /// Smooth Lerping
    /// </summary>
    /// <param name="a">Current Value</param>
    /// <param name="b">Target Value</param>
    /// <param name="decay">Value from 1 - 25, from slow to fast</param>
    /// <param name="dt"></param>
    /// <returns></returns>
    // public static float ExpDecayAngle(float a, float b, float decay, float dt)
    // {
    //     return b + Mathf.DeltaAngle(b, a) * Mathf.Exp(-decay * dt);
    // }
}
