using System.Collections.Generic;

namespace Vstk.Airlock
{
    internal interface IDataBatchesFactory
    {
        IEnumerable<IDataBatch> CreateBatches();
    }
}