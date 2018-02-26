using System;
using Vostok.Commons.Utilities;
using Vostok.Logging;

namespace Vostok.Airlock
{
    public class ParallelAirlockClient : IAirlockClient
    {
        private readonly AirlockClient[] clients;
        public AirlockClientCounters Counters { get; }

        public ParallelAirlockClient(AirlockConfig config, int parallelism, ILog log = null)
        {
            Counters = new AirlockClientCounters();
            clients = new AirlockClient[parallelism];

            for (var i = 0; i < parallelism; i++)
            {
                clients[i] = new AirlockClient(config, log, Counters);
            }
        }

        public void Push<T>(string routingKey, T item, DateTimeOffset? timestamp = null)
        {
            clients[ThreadSafeRandom.Next(clients.Length)].Push(routingKey, item, timestamp);
        }


        public void Dispose()
        {
            foreach (var client in clients)
            {
                client.Dispose();
            }
        }
    }
}
