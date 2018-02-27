using System;
using System.Collections.Generic;
using Vstk.Logging;

namespace Vstk.Airlock.Logging
{
    public sealed class LogEventData
    {
        public DateTimeOffset Timestamp { get; set; }

        public LogLevel Level { get; set; }

        public string Message { get; set; }

        public List<LogEventException> Exceptions { get; set; }

        public IDictionary<string, string> Properties { get; set; }
    }
}