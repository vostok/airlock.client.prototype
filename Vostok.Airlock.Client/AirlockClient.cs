using System;
using Vostok.Commons.Synchronization;
using Vostok.Commons.Utilities;
using Vostok.Logging;
using Vostok.Logging.Logs;
using Vostok.Metrics;

namespace Vostok.Airlock
{
    public class AirlockClient : IAirlockClient
    {
        public AirlockClientCounters Counters { get; }
        private readonly ILog log;

        private readonly AtomicBoolean isDisposed;
        private readonly AtomicBoolean pushAfterDisposeLogged;
        private readonly InternalAirlockClient[] airlockClients;
        private readonly int parallelizm;

        public AirlockClient(AirlockConfig config, ILog log = null, IMetricScope metricScope = null)
        {
            Counters = new AirlockClientCounters();
            AirlockConfigValidator.Validate(config);

            this.log = log = (log ?? new SilentLog()).ForContext(this);

            var memoryManager = new MemoryManager(
                config.MaximumMemoryConsumption.Bytes,
                (int) config.InitialPooledBufferSize.Bytes
            );
            var recordWriter = new RecordWriter(new RecordSerializer(config.MaximumRecordSize, log));

            parallelizm = config.Parallelism ?? 1;
            if (parallelizm <= 0)
                parallelizm = 1;
            airlockClients = new InternalAirlockClient[parallelizm];
            for (var i = 0; i < parallelizm; i++)
            {
                airlockClients[i] = new InternalAirlockClient(config, this.log, recordWriter, memoryManager, Counters);
            }
            isDisposed = new AtomicBoolean(false);
            pushAfterDisposeLogged = new AtomicBoolean(false);

            if (config.EnableMetrics)
            {
                if (metricScope == null)
                    log.Error("airlock metrics enabled but metricScope is not provided");
                else
                    SetupAirlockMetrics(metricScope);
            }
        }

        public void Push<T>(string routingKey, T item, DateTimeOffset? timestamp = null)
        {
            if (isDisposed)
            {
                LogPushAfterDispose(routingKey);
                return;
            }

            if (parallelizm > 1)
                airlockClients[ThreadSafeRandom.Next(airlockClients.Length)].Push(routingKey, item, timestamp);
            else
            {
                airlockClients[0].Push(routingKey, item, timestamp);
            }
        }

        public void Dispose()
        {
            if (!isDisposed.TrySetTrue())
                return;
            foreach (var client in airlockClients)
            {
                client.Dispose();
            }
        }

        private void SetupAirlockMetrics(IMetricScope rootScope)
        {
            var clock = MetricClocks.Get();
            var metricScope = rootScope.WithTag(MetricsTagNames.Type, "airlock");
            clock.Register(
                timestamp =>
                {
                    var lostItems = Counters.LostItems.Reset();
                    var sentItems = Counters.SentItems.Reset();
                    metricScope
                        .WriteMetric()
                        .SetTimestamp(timestamp)
                        .SetValue("lost-items", lostItems)
                        .SetValue("sent-items", sentItems)
                        .Commit();
                });
        }

        private void LogPushAfterDispose(string routingKey)
        {
            if (pushAfterDisposeLogged.TrySetTrue())
            {
                log.Warn($"Tried to push a message to routing key '{routingKey}' after dispose of AirlockClient");
            }
        }
    }
}