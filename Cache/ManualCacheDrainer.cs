using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abstractions;

namespace Cache
{
    public sealed class ManualCacheDrainer<T> where T : IView
    {
        private readonly IViewStore<T> _destinationViewStore;
        private readonly OutgoingCache<T> _outgoingCache;
        private readonly int _batchSize;
        
        public event OnSendingExceptionDelegate? OnSendingExceptionEvent;
        public event OnDrainFinishedDelegate? OnDrainFinishedEvent;

        public ManualCacheDrainer(
            IViewStore<T> destinationViewStore,
            OutgoingCache<T> outgoingCache,
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
            return cachedItems.Count;
        }

        private void DrainCache(IReadOnlyList<T> cachedItems)
        {
            foreach (var batch in cachedItems.Batch(_batchSize))
            {
                SendBatch(batch.ToList());
            }
            OnDrainFinishedEvent?.Invoke(cachedItems.Count);
        }

        private void SendBatch(IReadOnlyList<T> batch)
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
    }
}