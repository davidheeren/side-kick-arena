using System;
using Godot;

public static class Debug
{
    public static void AssertNotNull(object obj, string msg) => Assert(obj != null, msg);
    public static void Assert(bool cond, string msg)
#if DEBUG
    {
        if (cond) return;
        GD.PrintErr(msg);
        throw new ApplicationException($"Assert Failed: {msg}");
    }
#else
    {}
#endif
}
