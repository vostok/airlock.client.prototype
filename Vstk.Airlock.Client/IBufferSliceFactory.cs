using System.Collections.Generic;

namespace Vstk.Airlock
{
    internal interface IBufferSliceFactory
    {
        IEnumerable<BufferSlice> Cut(IBuffer buffer, int maximumSliceLength);
    }
}