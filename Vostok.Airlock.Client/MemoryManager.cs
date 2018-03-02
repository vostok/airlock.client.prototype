using Vostok.Commons.Synchronization;

namespace Vostok.Airlock
{
    internal class MemoryManager : IMemoryManager
    {
        private readonly int initialBuffersSize;
        private readonly long maxMemoryForBuffers;
        private readonly AtomicLong currentSize = new AtomicLong(0);

        public MemoryManager(long maxMemoryForBuffers, int initialBuffersSize)
        {
            this.maxMemoryForBuffers = maxMemoryForBuffers;
            this.initialBuffersSize = initialBuffersSize;
        }

        public bool TryCreateBuffer(out byte[] buffer)
        {
            return (buffer = TryReserveBytes(initialBuffersSize) ? new byte[initialBuffersSize] : null) != null;
        }

        public bool TryReserveBytes(int amount)
        {
            while (true)
            {
                var tCurrentSize = currentSize.Value;
                var newSize = tCurrentSize + amount;
                if (newSize <= maxMemoryForBuffers)
                {
                    if (currentSize.TrySet(newSize, tCurrentSize))
                        return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}