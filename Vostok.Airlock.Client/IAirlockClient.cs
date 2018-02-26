using System;

namespace Vostok.Airlock
{
    public interface IAirlockClient : IDisposable
    {
        void Push<T>(string routingKey, T item, DateTimeOffset? timestamp = null);
        AirlockClientCounters Counters { get; }
    }
}