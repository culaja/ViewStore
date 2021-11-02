using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ViewStore.Abstractions;

namespace ViewStore.Cache
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
            
            var drainStatistics = DrainCacheUntilEmptyInternal();
            
            
            if (drainStatistics.HasAnyItems)
            {
                OnDrainFinishedEvent?.Invoke(drainStatistics);
            }

            return drainStatistics.Count;
        }

        private DrainStatistics DrainCacheUntilEmptyInternal()
        {
            _stopwatch.Restart();
            try
            {
                var cachedItems = _outgoingCache.Renew();
                var drainAddedOrUpdatedCacheRetryCount = DrainAddedOrUpdatedCache(cachedItems);
                var drainDeletedCacheRetryCount = DrainDeletedCache(cachedItems);
                StoreCacheMetadata(cachedItems.LastGlobalVersion());
                return new DrainStatistics(
                    _stopwatch.Elapsed,
                    cachedItems.AddedOrUpdated.Count,
                    drainAddedOrUpdatedCacheRetryCount,
                    cachedItems.Deleted.Count,
                    drainDeletedCacheRetryCount);
            }
            finally
            {
                _stopwatch.Stop();
            }
            
        }

        private int DrainAddedOrUpdatedCache(CachedItems cachedItems)
        {
            var viewEnvelopeBatches = new ViewEnvelopeBatches(cachedItems.AddedOrUpdated, _batchSize);
            var numberOfRetries = 0;
            foreach (var batch in viewEnvelopeBatches)
            {
                numberOfRetries += SaveBatch(batch);
            }

            return numberOfRetries;
        }

        private int SaveBatch(IReadOnlyList<ViewEnvelope> batch, int numberOfRetries = 0)
        {
            try
            {
                _destinationViewStore.Save(batch);
            }
            catch (Exception e)
            {
                OnSendingExceptionEvent?.Invoke(e);
                Thread.Sleep(1000);
                return SaveBatch(batch, numberOfRetries + 1);
            }

            return numberOfRetries;
        }
        
        private int DrainDeletedCache(CachedItems cachedItems)
        {
            var deletedViewEnvelopeBatches = new DeletedViewEnvelopeBatches(cachedItems.Deleted, _batchSize);
            var numberOfRetries = 0;
            foreach (var batch in deletedViewEnvelopeBatches)
            {
                numberOfRetries += DeleteBatch(batch);
            }

            return numberOfRetries;
        }

        private int DeleteBatch(IReadOnlyList<DeletedViewEnvelope> batch, int numberOfRetries = 0)
        {
            try
            {
                _destinationViewStore.Delete(
                    batch.Select(i => i.ViewId),
                    batch.Max(i => i.GlobalVersion));
            }
            catch (Exception e)
            {
                OnSendingExceptionEvent?.Invoke(e);
                Thread.Sleep(1000);
                return DeleteBatch(batch, numberOfRetries + 1);
            }

            return numberOfRetries;
        }

        private void StoreCacheMetadata(GlobalVersion? largestGlobalVersion)
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
                StoreCacheMetadata(largestGlobalVersion);
            }
        }
    }
}