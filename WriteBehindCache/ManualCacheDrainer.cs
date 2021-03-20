using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ViewStore.Abstractions;

namespace ViewStore.WriteThroughCache
{
    internal sealed class ManualCacheDrainer
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
            var cachedItems = _outgoingCache.Renew();
            var viewBatches = new ViewEnvelopeBatches(cachedItems, _batchSize);
            DrainCache(viewBatches);
            StoreGlobalPosition(viewBatches.LargestGlobalVersion);
            return viewBatches.CountOfAllViewEnvelopes;
        }

        private void DrainCache(ViewEnvelopeBatches viewEnvelopeBatches)
        {
            foreach (var batch in viewEnvelopeBatches)
            {
                SendBatch(batch);
            }

            if (viewEnvelopeBatches.CountOfAllViewEnvelopes > 0)
            {
                OnDrainFinishedEvent?.Invoke(viewEnvelopeBatches);
            }
        }

        private void SendBatch(IReadOnlyList<ViewEnvelope> batch)
        {
            try
            {
                Task.WhenAll(batch.Select(view => _destinationViewStore.SaveAsync(view))).Wait();
            }
            catch (Exception e)
            {
                OnSendingExceptionEvent?.Invoke(e);
                Thread.Sleep(1000);
                SendBatch(batch);
            }
        }

        private void StoreGlobalPosition(GlobalVersion? largestGlobalVersion)
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
                StoreGlobalPosition(largestGlobalVersion);
            }
        }
    }
}