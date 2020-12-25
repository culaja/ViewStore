using System;
using Abstractions;

namespace Cache
{
    public sealed class ViewStoreCacheFactory<T> where T : IView
    {
        private IViewStore<T>? _realViewStore;
        private TimeSpan _cacheItemExpirationPeriod = TimeSpan.Zero;
        private TimeSpan _cacheDrainPeriod = TimeSpan.Zero;
        private int _cacheDrainBatchSize;
        
        public static ViewStoreCacheFactory<T> New() => new();

        public ViewStoreCacheFactory<T> For(IViewStore<T> viewStore)
        {
            _realViewStore = viewStore;
            return this;
        }

        public ViewStoreCacheFactory<T> WithCacheItemExpirationPeriod(TimeSpan timeSpan)
        {
            _cacheItemExpirationPeriod = timeSpan;
            return this;
        }

        public ViewStoreCacheFactory<T> WithCacheDrainPeriod(TimeSpan timeSpan)
        {
            _cacheDrainPeriod = timeSpan;
            return this;
        }

        public ViewStoreCacheFactory<T> WithCacheDrainBatchSize(int batchSize)
        {
            if (batchSize < 0)
            {
                throw new ArgumentException(nameof(batchSize));
            }

            _cacheDrainBatchSize = batchSize;
            return this;
        }

        public (IViewStore<T>, IDisposable) Build()
        {
            if (_realViewStore == null)
            {
                throw new ArgumentException(nameof(_realViewStore));
            }
            
            var outgoingCache = new OutgoingCache<T>();

            var automaticCacheDrainer = new AutomaticCacheDrainer<T>(
                new ManualCacheDrainer<T>(_realViewStore, outgoingCache, _cacheDrainBatchSize),
                _cacheDrainPeriod);

            automaticCacheDrainer.OnDrainFinishedEvent += Console.WriteLine;
            automaticCacheDrainer.OnSendingExceptionEvent += Console.WriteLine;

            return (
                new ViewStoreCache<T>(
                    _realViewStore,
                    outgoingCache,
                    _cacheItemExpirationPeriod),
                automaticCacheDrainer);
        }
    }
}