using Vostok.Logging;
using Vostok.Metrics;

namespace Vostok.Airlock
{
    public static class AirlockClientFactory
    {
        public static IAirlockClient CreateAirlockClient(AirlockConfig airlockConfig, ILog log, IMetricScope metricScope = null)
        {
            if (airlockConfig?.ApiKey == null || airlockConfig.ClusterProvider == null)
                return null;
            IAirlockClient airlockClient;
            var parallelism = airlockConfig.Parallelism;
            if (parallelism == null || parallelism.Value <= 1)
            {
                airlockClient = new AirlockClient(airlockConfig, log);
            }
            else
            {
                airlockClient = new ParallelAirlockClient(airlockConfig, parallelism.Value, log);
            }

            if (airlockConfig.EnableMetrics)
            {
                if (metricScope == null)
                    log.Error("airlock metrics enabled but metricScope is not provided");
                else
                    SetupAirlockMetrics(airlockClient, metricScope);
            }
            return airlockClient;
        }

        private static void SetupAirlockMetrics(IAirlockClient airlockClient, IMetricScope rootScope)
        {
            var clock = MetricClocks.Get();
            var metricScope = rootScope.WithTag(MetricsTagNames.Type, "airlock");
            clock.Register(
                timestamp =>
                {
                    var lostItems = airlockClient.Counters.LostItems.Reset();
                    var sentItems = airlockClient.Counters.SentItems.Reset();
                    metricScope
                        .WriteMetric()
                        .SetTimestamp(timestamp)
                        .SetValue("lost-items", lostItems)
                        .SetValue("sent-items", sentItems)
                        .Commit();
                });
        }

    }
}