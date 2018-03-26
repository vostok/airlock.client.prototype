﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Vostok.Commons.Extensions.UnitConvertions;
using Vostok.Commons.Synchronization;
using Vostok.Logging;

namespace Vostok.Airlock
{
    internal class DataSender : IDataSender
    {
        private readonly IDataBatchesFactory batchesFactory;
        private readonly IRequestSender requestSender;
        private readonly ILog log;
        private readonly AirlockClientCounters counters;
        private readonly BufferSliceTracker slicesTracker;

        public DataSender(
            IDataBatchesFactory batchesFactory, 
            IRequestSender requestSender, 
            ILog log,
            AirlockClientCounters counters)
        {
            this.batchesFactory = batchesFactory;
            this.requestSender = requestSender;
            this.log = log;
            this.counters = counters;

            slicesTracker = new BufferSliceTracker();
        }

        public async Task<DataSendResult> SendAsync()
        {
            slicesTracker.Reset();

            foreach (var batch in batchesFactory.CreateBatches())
            {
                var watch = Stopwatch.StartNew();

                var result = await requestSender.SendAsync(batch.SerializedMessage).ConfigureAwait(false);

                LogBatchSendResult(batch, result, watch.Elapsed);

                if (result == RequestSendResult.IntermittentFailure)
                    return DataSendResult.Backoff;

                log.Info("counters = " + counters);
                log.Info("counters.SentItems = " + counters.SentItems);
                log.Info("batch = " + batch);
                if (result == RequestSendResult.Success)
                    counters.SentItems.Add(batch.ItemsCount);

                if (result == RequestSendResult.DefinitiveFailure)
                    counters.LostItems.Add(batch.ItemsCount);
                
                TryDiscardSnapshots(batch);
            }

            return DataSendResult.Ok;
        }

        private void TryDiscardSnapshots(IDataBatch batch)
        {
            foreach (var slice in batch.ParticipatingSlices)
            {
                if (slicesTracker.TryCompleteSnapshot(slice))
                    slice.Buffer.DiscardSnapshot();
            }
        }

        private void LogBatchSendResult(IDataBatch batch, RequestSendResult result, TimeSpan elapsed)
        {
            if (result == RequestSendResult.Success)
            {
                log.Info($"Successfully sent batch of size {batch.SerializedMessage.Count.Bytes()} in {elapsed}.");
            }
            else
            {
                log.Warn($"Failed to send batch of size {batch.SerializedMessage.Count.Bytes()} with result '{result}' in {elapsed}.");
            }
        }
    }
}