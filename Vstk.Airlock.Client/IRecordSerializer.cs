using System;

namespace Vstk.Airlock
{
    internal interface IRecordSerializer
    {
        bool TrySerialize<T>(T item, IAirlockSerializer<T> serializer, DateTimeOffset timestamp, IBuffer buffer);
    }
}