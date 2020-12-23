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
            return cachedItems.Count;
        }

        private void DrainCache(IReadOnlyList<KeyValuePair<IViewId, IView>> cachedItems)
        {
            try
            {
                foreach (var batch in cachedItems.Batch(_batchSize))
                {
                    Task.WhenAll(batch.Select(kvp => _destinationViewStore.SaveAsync(kvp.Key, kvp.Value))).Wait();
                }
                OnDrainFinishedEvent?.Invoke(cachedItems.Count);
            }
            catch (Exception e)
            {
                OnSendingExceptionEvent?.Invoke(e);
                DrainCache(cachedItems);
            }
        }
    }
}