using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vstk.Airlock
{
    public interface IAirlockBatchClient
    {
        Task PushAsync<T>(string routingKey, IReadOnlyList<Tuple<T,DateTimeOffset>> items);
    }
}