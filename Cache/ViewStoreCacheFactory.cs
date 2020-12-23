using System;
using System.Collections.Concurrent;
using Abstractions;

namespace Cache
{
    public sealed class ViewStoreCacheFactory
    {
        private IViewStore? _realViewStore;
        private TimeSpan _cacheItemExpirationPeriod = TimeSpan.Zero;
        private TimeSpan _cacheDrainPeriod = TimeSpan.Zero;
        private int _cacheDrainBatchSize;
        
        public static ViewStoreCacheFactory New() => new();

        public ViewStoreCacheFactory For(IViewStore viewStore)
        {
            _realViewStore = viewStore;
            return this;
        }

        public ViewStoreCacheFactory WithCacheItemExpirationPeriod(TimeSpan timeSpan)
        {
            _cacheItemExpirationPeriod = timeSpan;
            return this;
        }

        public ViewStoreCacheFactory WithCacheDrainPeriod(TimeSpan timeSpan)
        {
            _cacheDrainPeriod = timeSpan;
            return this;
        }

        public ViewStoreCacheFactory WithCacheDrainBatchSize(int batchSize)
        {
            if (batchSize < 0)
            {
                throw new ArgumentException(nameof(batchSize));
            }

            _cacheDrainBatchSize = batchSize;
            return this;
        }

        public Tuple<IViewStore, IDisposable> Build()
        {
            if (_realViewStore == null)
            {
                throw new ArgumentException(nameof(_realViewStore));
            }
            
            var outgoingCache = new OutgoingCache();

            var automaticCacheDrainer = new AutomaticCacheDrainer(
                new ManualCacheDrainer(_realViewStore, outgoingCache, _cacheDrainBatchSize),
                _cacheDrainPeriod);

            automaticCacheDrainer.OnDrainFinishedEvent += Console.WriteLine;
            automaticCacheDrainer.OnSendingExceptionEvent += Console.WriteLine;

            return new Tuple<IViewStore, IDisposable>(
                new ViewStoreCache(
                    _realViewStore,
                    outgoingCache,
                    _cacheItemExpirationPeriod),
                automaticCacheDrainer);
        }
    }
}