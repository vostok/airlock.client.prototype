using System;
using Vostok.Clusterclient.Topology;
using Vostok.Commons.Extensions.UnitConvertions;

namespace Vostok.Airlock
{
    public class AirlockRequestSenderConfig
    {
        public IClusterProvider ClusterProvider { get; set; }

        public string ApiKey { get; set; }

        public TimeSpan RequestTimeout { get; set; } = 30.Seconds();

        public bool EnableTracing { get; set; } = false;
    }
}