using System;
using System.Collections.Generic;
using System.Threading;
using ViewStore.Abstractions;

namespace ViewStore.Cache
{
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

        public void AddOrUpdate(ViewEnvelope viewEnvelope)
        {
            lock (_sync)
            {
                if (_currentCache.Count > _throttleAfterCacheCount)
                {
                    ThrottleForOneSecond();
                }
                
                _currentCache.AddOrUpdate(viewEnvelope);
            }
        }
        
        public void AddOrUpdate(IEnumerable<ViewEnvelope> viewEnvelopes)
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

        public void Remove(string viewId, GlobalVersion globalVersion)
        {
            lock (_sync)
            {
                _currentCache.Remove(viewId, globalVersion);
            }
        }
        
        public void Remove(IEnumerable<string> viewIds, GlobalVersion globalVersion)
        {
            lock (_sync)
            {
                _currentCache.Remove(viewIds, globalVersion);
            }
        }

        public bool TryGetValue(string viewId, out ViewEnvelope viewEnvelope, out bool isDeleted)
        {
            lock (_sync)
            {
                return _currentCache.TryGetValue(viewId, out viewEnvelope, out isDeleted) || 
                       (isDeleted == false && _drainedCache.TryGetValue(viewId, out viewEnvelope, out isDeleted));
            }
        }

        public GlobalVersion? LastGlobalVersion()
        {
            lock (_sync)
            {
                return GlobalVersion.Max(
                    _currentCache.LastGlobalVersion(),
                    _drainedCache.LastGlobalVersion());
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
}