
// This is for network payloads
// Can push out of order items
// Access by tick id or currend index
// A modified circular buffer
using System;

public class PayloadTickBuffer<T>
{
    private readonly T[] data;
    public readonly int size;

    public int currentIndex { get; private set; } = -1;
    int newestTick = -1;

    public PayloadTickBuffer(int size)
    {
        this.size = size;
        data = new T[size];
    }

    public T Get(int tick)
    {
        if (IsTickStale(tick))
            throw new ArgumentOutOfRangeException($"Tick: {tick} is out of range");

        return data[tick % size];
    }

    public T GetCurrent()
    {
        return data[currentIndex];
    }

    public void Push(int tick, T item)
    {
        // Ignore stale items
        if (IsTickStale(tick))
            return;

        int newIndex = tick % size;
        data[newIndex] = item;

        if (tick > newestTick)
        {
            currentIndex = newIndex;
            newestTick = tick;
        }
    }

    // False if tick is too old
    public bool IsTickStale(int tick)
    {
        return newestTick - tick >= size;
    }
}
