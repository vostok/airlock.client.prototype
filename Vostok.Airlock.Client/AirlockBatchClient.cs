using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vostok.Commons.Binary;
using Vostok.Logging;

namespace Vostok.Airlock
{
    public class AirlockBatchClient : IAirlockBatchClient
    {
        private readonly RequestSender requestSender;
        private readonly InifiniteMemoryAllocator inifiniteMemoryAllocator = new InifiniteMemoryAllocator();

        public AirlockBatchClient(AirlockRequestSenderConfig config, ILog log)
        {
            requestSender = new RequestSender(config, log);
        }

        public async Task PushAsync<T>(string routingKey, IReadOnlyList<Tuple<T,DateTimeOffset>> items)
        {
            if (!AirlockSerializerRegistry.TryGet<T>(out var serializer))
                return;
            var buffer = new Buffer(new BinaryBufferWriter(items.Count*10), inifiniteMemoryAllocator);
            buffer.Write(AirlockClient.AirlockMessageVersion);
            buffer.Write(1);
            buffer.Write(routingKey);
            buffer.Write(items.Count);
            foreach (var tuple in items)
            {
                var unixTimestamp = tuple.Item2.ToUniversalTime().ToUnixTimeMilliseconds();
                buffer.Write(unixTimestamp);
                WriteRecord(buffer, serializer, tuple.Item1);
            }

            var serializedMessage = new ArraySegment<byte>(buffer.InternalBuffer, 0, buffer.Position);
            var result = await requestSender.SendAsync(serializedMessage).ConfigureAwait(false);

            if (result != RequestSendResult.Success)
                throw new AirlockException($"Sending error, routingKey='{routingKey}', items count={items.Count}", result);
        }

        private static void WriteRecord<T>(Buffer buffer, IAirlockSerializer<T> serializer, T item)
        {
            var payloadLengthPosition = buffer.Position;

            buffer.Write(0);

            var positionBeforeSerialization = buffer.Position;

            serializer.Serialize(item, buffer);

            var positionAfterSerialization = buffer.Position;

            var recordSize = positionAfterSerialization - positionBeforeSerialization;

            buffer.Position = payloadLengthPosition;

            buffer.Write(recordSize);

            buffer.Position = positionAfterSerialization;
        }
    }
}