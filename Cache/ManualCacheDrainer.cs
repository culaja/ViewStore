﻿using System;
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
            foreach (var batch in cachedItems.Batch(_batchSize))
            {
                SendBatch(batch.ToList());
            }
            OnDrainFinishedEvent?.Invoke(cachedItems.Count);
        }

        private void SendBatch(IReadOnlyList<KeyValuePair<IViewId, IView>> batch)
        {
            try
            {
                Task.WhenAll(batch.Select(kvp => _destinationViewStore.SaveAsync(kvp.Key, kvp.Value))).Wait();
            }
            catch (Exception e)
            {
                OnSendingExceptionEvent?.Invoke(e);
                SendBatch(batch);
            }
        }
    }
}