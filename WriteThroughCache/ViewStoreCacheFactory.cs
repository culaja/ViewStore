﻿using System;
using System.Collections.Generic;
using ViewStore.Abstractions;

namespace ViewStore.WriteThroughCache
{
    public sealed class ViewStoreCacheFactory
    {
        private IViewStore? _realViewStore;
        private TimeSpan _readCacheExpirationPeriod = TimeSpan.Zero;
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

        public ViewStoreCacheFactory WithReadCacheExpirationPeriod(TimeSpan timeSpan)
        {
            _readCacheExpirationPeriod = timeSpan;
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

            automaticCacheDrainer.OnDrainFinishedEvent += views => _cacheDrainedCallback?.Invoke(views);
            automaticCacheDrainer.OnSendingExceptionEvent += exception => _onDrainAttemptFailedCallback?.Invoke(exception);

            var viewStoreCacheInternal = new ViewStoreCacheInternal(
                _realViewStore,
                outgoingCache);

            return new ViewStoreCache(
                viewStoreCacheInternal,
                automaticCacheDrainer);
        }
    }
}