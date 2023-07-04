using System;
using System.Threading;

namespace Umbrella.RabbitMQ.Buffers;

public partial class RingBuffer<T>
{
    public class AccquisitonController : IAccquisitonController<T>
    {
        private readonly RingBuffer<T> ringBuffer;

        internal AccquisitonController(RingBuffer<T> ringBuffer, TimeSpan waitTime)
        {
            this.ringBuffer = ringBuffer;
            T item;
            while (this.ringBuffer.VirtualCount == 0 || this.ringBuffer.buffer.TryDequeue(out item) is false)
            {
#if DEBUG
                Console.WriteLine($"RingBuffer | Waiting.. VirtualCount:{this.ringBuffer.VirtualCount} Capacity:{this.ringBuffer.Capacity}");
#endif
                Thread.Sleep(waitTime);
            }

#if DEBUG
            Console.WriteLine($"RingBuffer | Acquired! VirtualCount:{this.ringBuffer.VirtualCount} Capacity:{this.ringBuffer.Capacity}");
#endif
            Instance = item;
        }

        public T Instance { get; }

        public void Dispose()
        {
            if (ringBuffer.checkFunc(Instance))
            {
                ringBuffer.buffer.Enqueue(Instance);
            }
            else
            {
                ringBuffer.buffer.Enqueue(ringBuffer.factoryFunc());
#if DEBUG
                Console.WriteLine($"RingBuffer | Replacing....! VirtualCount:{ringBuffer.VirtualCount} Capacity:{ringBuffer.Capacity}");
#endif
                ringBuffer.disposeAction(Instance);
            }

            GC.SuppressFinalize(this);
        }
    }
}