using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace ViewStore.Cache.Cache;

public sealed class ViewStoreCacheBuilder
{
    private IDatabaseProvider? _databaseProvider;
    private TimeSpan _cacheDrainPeriod = TimeSpan.FromSeconds(5);
    private int _cacheDrainBatchSize = 1000;
    private int _throttleAfterCacheCount = 50000;
    private readonly List<Action<DrainStatistics>> _cacheDrainedCallbacks = new();
    private Action<ThrottleStatistics>? _throttlingCallback;
    private Action<Exception>? _onDrainAttemptFailedCallback;
    private bool _isBackgroundWorker;
        
    private MemoryCache _memoryCache = MemoryCache.Default;
    private TimeSpan _readCacheExpirationPeriod = TimeSpan.FromSeconds(10);

    public static ViewStoreCacheBuilder New() => new();

    public ViewStoreCacheBuilder WithDatabaseProvider(IDatabaseProvider databaseProvider)
    {
        _databaseProvider = new DatabaseProviderPreparationDecorator(databaseProvider);
        return this;
    }

    public ViewStoreCacheBuilder WithReadMemoryCache(MemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        return this;
    }
        
    public ViewStoreCacheBuilder WithReadCacheExpirationPeriod(TimeSpan readCacheExpirationPeriod)
    {
        _readCacheExpirationPeriod = readCacheExpirationPeriod;
        return this;
    }

    public ViewStoreCacheBuilder WithCacheDrainPeriod(TimeSpan timeSpan)
    {
        _cacheDrainPeriod = timeSpan;
        return this;
    }

    public ViewStoreCacheBuilder WithCacheDrainBatchSize(int batchSize)
    {
        if (batchSize < 0)
        {
            throw new ArgumentException(nameof(batchSize));
        }

        _cacheDrainBatchSize = batchSize;
        return this;
    }
        
    public ViewStoreCacheBuilder UseBackgroundWorker()
    {
        _isBackgroundWorker = true;
        return this;
    }

    public ViewStoreCacheBuilder WithThrottleAfterCacheCount(int throttleAfterCacheCount)
    {
        if (throttleAfterCacheCount <= 0)
        {
            throw new ArgumentException(nameof(throttleAfterCacheCount));
        }
            
        _throttleAfterCacheCount = throttleAfterCacheCount;
        return this;
    }

    public ViewStoreCacheBuilder UseCallbackWhenDrainFinished(Action<DrainStatistics> callback)
    {
        _cacheDrainedCallbacks.Add(callback);
        return this;
    }
        
    public ViewStoreCacheBuilder UseCallbackOnThrottling(Action<ThrottleStatistics> throttlingCallback)
    {
        _throttlingCallback = throttlingCallback;
        return this;
    }

    public ViewStoreCacheBuilder UseCallbackOnDrainAttemptFailed(Action<Exception> callback)
    {
        _onDrainAttemptFailedCallback = callback;
        return this;
    }

    public ViewStoreCache Build()
    {
        if (_databaseProvider == null)
        {
            throw new ArgumentException(nameof(_databaseProvider));
        }
            
        var outgoingCache = new OutgoingCache(_throttleAfterCacheCount, _throttlingCallback);

        var automaticCacheDrainer = new AutomaticCacheDrainer(
            new ManualCacheDrainer(_databaseProvider, outgoingCache, _cacheDrainBatchSize),
            _cacheDrainPeriod,
            _isBackgroundWorker);

        automaticCacheDrainer.OnDrainFinishedEvent += ds =>
        {
            foreach (var callback in _cacheDrainedCallbacks)
            {
                callback(ds);
            }
        };
        automaticCacheDrainer.OnSendingExceptionEvent += exception => _onDrainAttemptFailedCallback?.Invoke(exception);

        var readThroughViewStoreCache = new ReadThroughDatabaseProviderCache(
            _memoryCache,
            _readCacheExpirationPeriod,
            _databaseProvider);

        var viewStoreCacheInternal = new ViewStoreCacheInternal(
            readThroughViewStoreCache,
            outgoingCache);

        return new ViewStoreCache(
            viewStoreCacheInternal,
            automaticCacheDrainer);
    }
}