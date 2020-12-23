using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Abstractions;

namespace Cache
{
    public sealed class ManualCacheDrainer
    {
        private readonly IViewStore _destinationViewStore;
        private readonly ConcurrentDictionary<IViewId, IView> _outgoingCache;
        private readonly int _batchSize;

        public ManualCacheDrainer(
            IViewStore destinationViewStore,
            ConcurrentDictionary<IViewId, IView> outgoingCache,
            int batchSize)
        {
            _destinationViewStore = destinationViewStore;
            _outgoingCache = outgoingCache;
            _batchSize = batchSize;
        }

        public int ItemsToDrain => _outgoingCache.Count;

        public void DrainCache()
        {
            SendOutgoingCache();
            ClearOutgoingCache();
        }
        
        private void SendOutgoingCache()
        {
            foreach (var batch in _outgoingCache.Batch(_batchSize))
            {
                Task.WhenAll(batch.Select(kvp => _destinationViewStore.SaveAsync<IView>(kvp.Key, kvp.Value))).Wait();
            }
        }

        private void ClearOutgoingCache()
        {
            _outgoingCache.Clear();
        }
    }
}