using System;
using System.Collections.Concurrent;
using Vstk.Commons.Synchronization;
using Vstk.Logging;

namespace Vstk.Airlock
{
    internal class InternalAirlockClient
    {
        private readonly AirlockClientCounters counters;
        private readonly AirlockConfig config;
        private readonly MemoryManager memoryManager;
        private readonly RecordWriter recordWriter;
        private readonly ConcurrentDictionary<string, IBufferPool> bufferPools;
        private readonly DataSenderDaemon dataSenderDaemon;

        private readonly AtomicBoolean isDisposed;

        public InternalAirlockClient(AirlockConfig config, ILog log, RecordWriter recordWriter, MemoryManager memoryManager, AirlockClientCounters counters)
        {
            this.counters = counters;
            AirlockConfigValidator.Validate(config);

            this.config = config;

            this.recordWriter = recordWriter;
            this.memoryManager = memoryManager;

            bufferPools = new ConcurrentDictionary<string, IBufferPool>();

            var requestSender = new RequestSender(config, log);
            var commonBatchBuffer = new byte[config.MaximumBatchSizeToSend.Bytes];
            var dataBatchesFactory = new DataBatchesFactory(
                bufferPools,
                commonBatchBuffer
            );
            var dataSender = new DataSender(
                dataBatchesFactory,
                requestSender,
                log,
                this.counters
            );

            dataSenderDaemon = new DataSenderDaemon(dataSender, config, log);
            isDisposed = new AtomicBoolean(false);
        }

        public void Push<T>(string routingKey, T item, DateTimeOffset? timestamp = null)
        {
            if (!AirlockSerializerRegistry.TryGet<T>(out var serializer))
                return;

            if (!recordWriter.TryWrite(
                item,
                serializer,
                timestamp ?? DateTimeOffset.UtcNow,
                ObtainBufferPool(routingKey)))
            {
                counters.LostItems.Add();
            }
        }

        public void Dispose()
        {
            if (isDisposed.TrySetTrue())
            {
                dataSenderDaemon.Dispose();
            }
        }

        private IBufferPool ObtainBufferPool(string routingKey)
        {
            return bufferPools.GetOrAdd(
                routingKey,
                _ => new BufferPool(
                    memoryManager,
                    config.InitialPooledBuffersCount
                ));
        }
    }
}