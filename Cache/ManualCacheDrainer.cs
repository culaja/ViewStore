using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abstractions;

namespace Cache
{
    public sealed class ManualCacheDrainer
    {
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

        public int TryDrainCache()
        {
            var cachedItems = _outgoingCache.Clear();
            DrainCache(cachedItems);
            StoreGlobalPosition(cachedItems);
            return cachedItems.Count;
        }

        private void DrainCache(IReadOnlyList<IView> cachedItems)
        {
            foreach (var batch in cachedItems.Batch(_batchSize))
            {
                SendBatch(batch.ToList());
            }

            if (cachedItems.Count > 0)
            {
                OnDrainFinishedEvent?.Invoke(cachedItems);
            }
        }

        private void SendBatch(IReadOnlyList<IView> batch)
        {
            try
            {
                Task.WhenAll(batch.Select(view => _destinationViewStore.SaveAsync(view))).Wait();
            }
            catch (Exception e)
            {
                OnSendingExceptionEvent?.Invoke(e);
                SendBatch(batch);
            }
        }

        private void StoreGlobalPosition(IReadOnlyList<IView> cachedItems)
        {
            try
            {
                var latestCachedView = cachedItems.OrderByDescending(v => v.GlobalVersion).FirstOrDefault();
                if (latestCachedView != null)
                {
                    _destinationViewStore.Save(ViewMetaData.Of(latestCachedView.GlobalVersion));
                }
            }
            catch (Exception e)
            {
                OnSendingExceptionEvent?.Invoke(e);
                StoreGlobalPosition(cachedItems);
            }
        }
    }
}