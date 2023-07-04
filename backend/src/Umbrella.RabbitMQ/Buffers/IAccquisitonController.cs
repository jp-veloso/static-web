using System;

namespace Umbrella.RabbitMQ.Buffers;

public interface IAccquisitonController<out T> : IDisposable
{
    T Instance { get; }
}