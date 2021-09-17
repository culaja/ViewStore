using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ViewStore.Abstractions;

namespace ViewStore.WriteBehindCache
{
    internal sealed class ManualCacheDrainer
    {
        private readonly Stopwatch _stopwatch = new();
        private readonly IViewStore _destinationViewStore;
        private readonly OutgoingCache _outgoingCache;
        private readonly int _batchSize;
        
        public event OnSendingExceptionDelegate? OnSendingExceptionEvent;
        public event OnDrainFinishedDelegate? OnDrainFinishedEvent;

        public ManualCacheDrainer(
            IViewStore destinationViewStore,
            OutgoingCache outgoingCache,
            int batchSize)
        {
            _destinationViewStore = destinationViewStore;
            _outgoingCache = outgoingCache;
            _batchSize = batchSize;
        }

        public int DrainCacheUntilEmpty()
        {
            _stopwatch.Restart();
            var (drainedViewCount, numberOfRetries) = DrainCacheUntilEmptyInternal();
            _stopwatch.Stop();
            
            if (drainedViewCount > 0)
            {
                OnDrainFinishedEvent?.Invoke(new DrainStatistics(_stopwatch.Elapsed,drainedViewCount,numberOfRetries));
            }

            return drainedViewCount;
        }

        private (int, int) DrainCacheUntilEmptyInternal()
        {
            var cachedItems = _outgoingCache.Renew();
            var viewBatches = new ViewEnvelopeBatches(cachedItems, _batchSize);
            var drainCacheRetryCount = DrainCache(viewBatches);
            var storeCacheMetadataRetryCount = StoreCacheMetadata(viewBatches.LargestGlobalVersion);
            return (viewBatches.CountOfAllViewEnvelopes, drainCacheRetryCount + storeCacheMetadataRetryCount);
        }

        private int DrainCache(ViewEnvelopeBatches viewEnvelopeBatches)
        {
            var numberOfRetries = 0;
            foreach (var batch in viewEnvelopeBatches)
            {
                numberOfRetries += SendBatch(batch);
            }

            return numberOfRetries;
        }

        private int SendBatch(IReadOnlyList<ViewEnvelope> batch, int numberOfRetries = 0)
        {
            try
            {
                _destinationViewStore.SaveAsync(batch).Wait();
            }
            catch (Exception e)
            {
                OnSendingExceptionEvent?.Invoke(e);
                Thread.Sleep(1000);
                return SendBatch(batch, numberOfRetries + 1);
            }

            return numberOfRetries;
        }

        private int StoreCacheMetadata(GlobalVersion? largestGlobalVersion, int numberOfRetries = 0)
        {
            try
            {
                if (largestGlobalVersion != null)
                {
                    _destinationViewStore.Save(ViewMetaData.MetaDataEnvelopeFor(largestGlobalVersion.Value));
                }
            }
            catch (Exception e)
            {
                OnSendingExceptionEvent?.Invoke(e);
                Thread.Sleep(1000);
                StoreCacheMetadata(largestGlobalVersion, numberOfRetries + 1);
            }

            return numberOfRetries;
        }
    }
}