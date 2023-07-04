using System;
using System.Collections.Concurrent;

namespace Umbrella.RabbitMQ.Buffers;

public partial class RingBuffer<T>
{
    private readonly ConcurrentQueue<T> buffer;
    private readonly Func<T, bool> checkFunc;
    private readonly Action<T> disposeAction;
    private readonly Func<T> factoryFunc;

    public int Capacity { get; }

    public TimeSpan WaitTime { get; }

    public int VirtualCount { get; private set; }

    public RingBuffer(int capacity, Func<T> factoryFunc, Func<T, bool> checkFunc, Action<T> disposeAction) :
        this(capacity, factoryFunc, checkFunc, disposeAction, TimeSpan.FromMilliseconds(50))
    {
    }

    public RingBuffer(int capacity, Func<T> factoryFunc, Func<T, bool> checkFunc, Action<T> disposeAction,
                      TimeSpan waitTime)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero");
        }

        this.factoryFunc =
            factoryFunc ?? throw new ArgumentNullException(nameof(factoryFunc), "factoryFunc can't be null");
        this.checkFunc = checkFunc ?? throw new ArgumentNullException(nameof(checkFunc), "checkFunc can't be null");
        this.disposeAction = disposeAction ??
                             throw new ArgumentNullException(nameof(disposeAction), "disposeAction can't be null");
        Capacity = capacity;
        WaitTime = waitTime;
        VirtualCount = 0;
        buffer = new ConcurrentQueue<T>();

        for (int i = 1; i <= Capacity; i++)
        {
            buffer.Enqueue(this.factoryFunc());
            VirtualCount++;
        }
    }

    public virtual IAccquisitonController<T> Accquire()
    {
        return new AccquisitonController(this, WaitTime);
    }
}