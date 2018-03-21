using System.Collections.Generic;

namespace Vstk.Airlock
{
    internal interface IBufferPool
    {
        bool TryAcquire(out IBuffer buffer);

        void Release(IBuffer buffer);

        List<IBuffer> GetSnapshot();
    }
}