using Vstk.Metrics.Meters;

namespace Vstk.Airlock
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