using Vostok.Metrics.Meters;

namespace Vostok.Airlock
{
    public class AirlockClientCounters
    {
        public Counter LostItems { get; }
        public Counter SentItems { get; }

        public AirlockClientCounters()
        {
            LostItems = new Counter();
            SentItems = new Counter();
        }
    }
}