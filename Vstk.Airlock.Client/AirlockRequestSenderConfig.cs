using System;
using Vstk.Clusterclient.Topology;
using Vstk.Commons.Extensions.UnitConvertions;

namespace Vstk.Airlock
{
    public class AirlockRequestSenderConfig
    {
        public IClusterProvider ClusterProvider { get; set; }

        public string ApiKey { get; set; }

        public TimeSpan RequestTimeout { get; set; } = 30.Seconds();

        public bool EnableTracing { get; set; } = false;
    }
}