using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using MoreLinq;
using NUnit.Framework;
using Vostok.Airlock.Logging;
using Vostok.Clusterclient.Topology;
using Vostok.Commons.Extensions.UnitConvertions;
using Vostok.Logging;
using Vostok.Logging.Logs;

namespace Vostok.Airlock.Client.Tests
{
    [Ignore("Explicit attribute does not work in VS + Resharper")]
    public class Integration_Tests
    {
        private readonly ConsoleLog log = new ConsoleLog();

        [Test]
        public void PushLogEventsToAirlock()
        {
            var routingKey = RoutingKey.Create("vostok", "ci", "core", RoutingKey.LogsSuffix);
            var events = GenerateLogEvens(count: 10000);
            PushToAirlock(routingKey, events, e => e.Timestamp);
        }

        [Test]
        public void PushBatchLogEventsToAirlock()
        {
            var routingKey = RoutingKey.Create("vostok", "ci", "core", RoutingKey.LogsSuffix);
            var events = GenerateLogEvens(count: 100000);
            foreach (var batch in events.Batch(10000))
            {
                PushBatchToAirlock(routingKey, batch.ToArray(), e => e.Timestamp);
            }
        }

        private static LogEventData[] GenerateLogEvens(int count)
        {
            var utcNow = DateTimeOffset.UtcNow;
            return Enumerable.Range(0, count)
                             .Select(i => new LogEventData
                             {
                                 Message = "Testing AirlockClient" + i,
                                 Level = LogLevel.Debug,
                                 Timestamp = utcNow.AddSeconds(-i*10),
                                 Properties = new Dictionary<string, string>()
                             }).ToArray();
        }

        private void PushToAirlock<T>(string routingKey, T[] events, Func<T, DateTimeOffset> getTimestamp)
        {
            log.Debug($"Pushing {events.Length} events to airlock");
            var sw = Stopwatch.StartNew();
            IAirlockClient airlockClient;
            using (airlockClient = CreateAirlockClient())
            {
                foreach (var @event in events)
                    airlockClient.Push(routingKey, @event);
            }

            var lostItems = airlockClient.Counters.LostItems.GetValue();
            var sentItems = airlockClient.Counters.SentItems.GetValue();
            log.Debug($"SentItemsCount: {sentItems}, LostItemsCount: {lostItems}, Elapsed: {sw.Elapsed}");
            lostItems.Should().Be(0);
            sentItems.Should().Be(events.Length);
        }

        private void PushBatchToAirlock<T>(string routingKey, T[] events, Func<T, DateTimeOffset> getTimestamp)
        {
            log.Debug($"Pushing {events.Length} events to airlock");
            var sw = Stopwatch.StartNew();
            var airlockClient = CreateBatchAirlockClient();
            var eventsBatch = events.Select(x => new Tuple<T, DateTimeOffset>(x, getTimestamp(x))).ToArray();
            airlockClient.PushAsync(routingKey, eventsBatch).Wait();
            log.Debug($"SentItemsCount: {events.Length}, Elapsed: {sw.Elapsed}");
        }

        private IAirlockClient CreateAirlockClient()
        {
            var airlockConfig = CreateAirlockConfig();
            return new AirlockClient(airlockConfig, log.FilterByLevel(LogLevel.Warn));
        }
        private AirlockBatchClient CreateBatchAirlockClient()
        {
            var airlockConfig = CreateAirlockConfig();
            return new AirlockBatchClient(airlockConfig, log.FilterByLevel(LogLevel.Warn));
        }

        private static AirlockConfig CreateAirlockConfig()
        {
            var airlockConfig = new AirlockConfig
            {
                ApiKey = "UniversalApiKey",
                ClusterProvider = new FixedClusterProvider(new Uri("http://localhost:6306")),
                SendPeriod = TimeSpan.FromSeconds(2),
                SendPeriodCap = TimeSpan.FromMinutes(5),
                RequestTimeout = TimeSpan.FromSeconds(30),
                MaximumRecordSize = 1.Kilobytes(),
                MaximumBatchSizeToSend = 300.Megabytes(),
                MaximumMemoryConsumption = 300.Megabytes()*10,
                InitialPooledBufferSize = 10.Megabytes(),
                InitialPooledBuffersCount = 10,
                EnableTracing = false,
                EnableMetrics = false,
                Parallelism = 10
            };
            return airlockConfig;
        }
    }
}