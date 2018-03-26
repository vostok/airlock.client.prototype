using System;
using Vostok.Commons.Extensions.UnitConvertions;
using Vostok.Commons.Model;

namespace Vostok.Airlock
{
    public class AirlockConfig : AirlockRequestSenderConfig
    {
        public DataSize MaximumRecordSize { get; set; } = 1.Megabytes();

        public DataSize MaximumMemoryConsumption { get; set; } = 128.Megabytes();

        public DataSize MaximumBatchSizeToSend { get; set; } = 4.Megabytes();

        public DataSize InitialPooledBufferSize { get; set; } = 16.Kilobytes();

        public int InitialPooledBuffersCount { get; set; } = 32;

        public TimeSpan SendPeriod { get; set; } = 2.Seconds();

        public TimeSpan SendPeriodCap { get; set; } = 5.Minutes();

        public int? Parallelism { get; set; } = 1;

        public bool EnableMetrics { get; set; } = true;
    }
}