using System;
using System.Linq;
using Vstk.Commons;
using Vstk.Logging;
using Vstk.Logging.Logs;

namespace Vstk.Airlock.Logging
{
    public class AirlockLog : ILog
    {
        private const int maxMessageLength = 32 * 1024;
        private readonly IAirlockClient airlockClient;
        private readonly string routingKey;

        public AirlockLog(IAirlockClient airlockClient, string routingKey)
        {
            this.airlockClient = airlockClient;
            this.routingKey = routingKey;
        }

        public void Log(LogEvent logEvent)
        {
            if (airlockClient == null || string.IsNullOrEmpty(routingKey))
                return;

            var logEventData = new LogEventData
            {
                Timestamp = DateTimeOffset.UtcNow, // todo (spaceorc, 15.02.2018) возможно, надо сделать поле Timestamp в logEvent?
                Level = logEvent.Level,
                Message = LogEventFormatter.FormatMessage(logEvent.MessageTemplate, logEvent.MessageParameters),
                Exceptions = logEvent.Exception.Parse(), // todo (andrew, 17.01.2018): maybe truncate if serialized Exceptions list has size > 32 kb
                Properties = logEvent.Properties.ToDictionary(x => x.Key, x => x.Value.ToString())
            };
            // todo (spaceorc, 13.10.2017) make "host" constant somewhere in Vostok.Core/LogPropertyNames.cs
            logEventData.Properties["host"] = HostnameProvider.Get();

            airlockClient.Push(routingKey, logEventData, logEventData.Timestamp);
        }

        public bool IsEnabledFor(LogLevel level) => true;
    }
}