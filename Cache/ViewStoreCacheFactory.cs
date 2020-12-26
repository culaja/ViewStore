﻿using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using Abstractions;

namespace Cache
{
    public sealed class ViewStoreCacheFactory
    {
        private IViewStore? _realViewStore;
        private TimeSpan _cacheItemExpirationPeriod = TimeSpan.Zero;
        private TimeSpan _cacheDrainPeriod = TimeSpan.Zero;
        private int _cacheDrainBatchSize;
        private Action<IReadOnlyList<IView>>? _cacheDrainedCallback;
        private Action<Exception>? _onDrainAttemptFailedCallback;

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

        public ViewStoreCacheFactory UseCallbackWhenDrainFinished(Action<IReadOnlyList<IView>> callback)
        {
            _cacheDrainedCallback = callback;
            return this;
        }

        public ViewStoreCacheFactory UseCallbackOnDrainAttemptFailed(Action<Exception> callback)
        {
            _onDrainAttemptFailedCallback = callback;
            return this;
        }

        public (IViewStore, IDisposable) Build()
        {
            if (_realViewStore == null)
            {
                throw new ArgumentException(nameof(_realViewStore));
            }
            
            var outgoingCache = new OutgoingCache();

            var automaticCacheDrainer = new AutomaticCacheDrainer(
                new ManualCacheDrainer(_realViewStore, outgoingCache, _cacheDrainBatchSize),
                _cacheDrainPeriod);

            automaticCacheDrainer.OnDrainFinishedEvent += views => _cacheDrainedCallback?.Invoke(views);
            automaticCacheDrainer.OnSendingExceptionEvent += exception => _onDrainAttemptFailedCallback?.Invoke(exception);

            return (
                new ViewStoreCache(
                    _realViewStore,
                    MemoryCache.Default, 
                    outgoingCache,
                    _cacheItemExpirationPeriod),
                automaticCacheDrainer);
        }
    }
}