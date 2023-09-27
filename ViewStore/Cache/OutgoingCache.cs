using System;
using System.Collections.Generic;
using System.Threading;

namespace ViewStore.Cache;

internal sealed class OutgoingCache
{
    private readonly object _sync = new();
    private CachedItems _currentCache = new();
    private CachedItems _drainedCache = new();
        
    private readonly int _throttleAfterCacheCount;
    private readonly Action<ThrottleStatistics>? _throttlingCallback;

    public OutgoingCache(int throttleAfterCacheCount, Action<ThrottleStatistics>? throttlingCallback)
    {
        _throttleAfterCacheCount = throttleAfterCacheCount;
        _throttlingCallback = throttlingCallback;
    }

    public void AddOrUpdate(ViewRecord viewRecord)
    {
        lock (_sync)
        {
            if (_currentCache.Count > _throttleAfterCacheCount)
            {
                ThrottleForOneSecond();
            }
                
            _currentCache.AddOrUpdate(viewRecord);
        }
    }
        
    public void AddOrUpdate(IEnumerable<ViewRecord> viewEnvelopes)
    {
        lock (_sync)
        {
            if (_currentCache.Count > _throttleAfterCacheCount)
            {
                ThrottleForOneSecond();
            }
                
            _currentCache.AddOrUpdate(viewEnvelopes);
        }
    }

    private void ThrottleForOneSecond()
    {
        var throttlingPeriod = TimeSpan.FromSeconds(1);
        var throttleDetails = new ThrottleStatistics(_currentCache.Count, _throttleAfterCacheCount, throttlingPeriod);
        _throttlingCallback?.Invoke(throttleDetails);
        Thread.Sleep(1000);
    }

    public void Remove(string viewId, long globalVersion)
    {
        lock (_sync)
        {
            _currentCache.Remove(viewId, globalVersion);
        }
    }
        
    public void Remove(IEnumerable<string> viewIds, long globalVersion)
    {
        lock (_sync)
        {
            _currentCache.Remove(viewIds, globalVersion);
        }
    }

    public bool TryGetValue(string viewId, out ViewRecord viewRecord, out bool isDeleted)
    {
        lock (_sync)
        {
            return _currentCache.TryGetValue(viewId, out viewRecord, out isDeleted) || 
                   (isDeleted == false && _drainedCache.TryGetValue(viewId, out viewRecord, out isDeleted));
        }
    }

    public long? LastGlobalVersion()
    {
        lock (_sync)
        {
            var currentCacheLastGlobalVersion = _currentCache.LastGlobalVersion();
            var drainedCacheLastGlobalVersion = _drainedCache.LastGlobalVersion();
            if (currentCacheLastGlobalVersion == null && drainedCacheLastGlobalVersion == null)
            {
                return null;
            }
                    
            return Math.Max(
                currentCacheLastGlobalVersion ?? 0L,
                drainedCacheLastGlobalVersion ?? 0L);
        }
    }

    public CachedItems Renew()
    {
        lock (_sync)
        {
            _drainedCache = _currentCache;
            _currentCache = new();
            return _drainedCache;
        }
    }
}