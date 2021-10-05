using System;
using System.Runtime.Caching;
using ViewStore.Abstractions;

namespace ViewStore.Cache
{
    public sealed class ViewStoreCacheFactory
    {
        private IViewStore? _realViewStore;
        private TimeSpan _cacheDrainPeriod = TimeSpan.FromSeconds(5);
        private int _cacheDrainBatchSize;
        private Action<DrainStatistics>? _cacheDrainedCallback;
        private Action<Exception>? _onDrainAttemptFailedCallback;
        
        private MemoryCache _memoryCache = MemoryCache.Default;
        private TimeSpan _readCacheExpirationPeriod = TimeSpan.FromSeconds(10);

        public static ViewStoreCacheFactory New() => new();

        public ViewStoreCacheFactory For(IViewStore viewStore)
        {
            _realViewStore = viewStore;
            return this;
        }

        public ViewStoreCacheFactory WithReadMemoryCache(MemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            return this;
        }
        
        public ViewStoreCacheFactory WithReadCacheExpirationPeriod(TimeSpan readCacheExpirationPeriod)
        {
            _readCacheExpirationPeriod = readCacheExpirationPeriod;
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

        public ViewStoreCacheFactory UseCallbackWhenDrainFinished(Action<DrainStatistics> callback)
        {
            _cacheDrainedCallback = callback;
            return this;
        }

        public ViewStoreCacheFactory UseCallbackOnDrainAttemptFailed(Action<Exception> callback)
        {
            _onDrainAttemptFailedCallback = callback;
            return this;
        }

        public ViewStoreCache Build()
        {
            if (_realViewStore == null)
            {
                throw new ArgumentException(nameof(_realViewStore));
            }
            
            var outgoingCache = new OutgoingCache();

            var automaticCacheDrainer = new AutomaticCacheDrainer(
                new ManualCacheDrainer(_realViewStore, outgoingCache, _cacheDrainBatchSize),
                _cacheDrainPeriod);

            automaticCacheDrainer.OnDrainFinishedEvent += ds => _cacheDrainedCallback?.Invoke(ds);
            automaticCacheDrainer.OnSendingExceptionEvent += exception => _onDrainAttemptFailedCallback?.Invoke(exception);

            var readThroughViewStoreCache = new ReadThroughViewStoreCache(
                _memoryCache,
                _readCacheExpirationPeriod,
                _realViewStore);

            var viewStoreCacheInternal = new ViewStoreCacheInternal(
                readThroughViewStoreCache,
                outgoingCache);

            return new ViewStoreCache(
                viewStoreCacheInternal,
                automaticCacheDrainer);
        }
    }
}