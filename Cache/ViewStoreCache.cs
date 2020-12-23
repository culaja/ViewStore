using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Abstractions;

namespace Cache
{
    public sealed class ViewStoreCache : IViewStore
    {
        private readonly IViewStore _next;
        private readonly MemoryCache _readCache = MemoryCache.Default;
        private readonly OutgoingCache _outgoingCache;
        private readonly TimeSpan _expirationPeriod;

        public ViewStoreCache(
            IViewStore next,
            OutgoingCache outgoingCache,
            TimeSpan expirationPeriod)
        {
            _next = next;
            _outgoingCache = outgoingCache;
            _expirationPeriod = expirationPeriod;
        }
        
        public Task<T?> ReadAsync<T>(IViewId viewId) where T : IView
        {
            CacheItem? optionalCacheItem = _readCache.GetCacheItem(viewId.ToString());
            if (optionalCacheItem != null)
            {
                return Task.FromResult((T?) optionalCacheItem.Value);
            }
            
            if (_outgoingCache.TryGetValue(viewId, out var view))
            {
                return Task.FromResult((T?) view);
            }
            
            return _next.ReadAsync<T>(viewId);
        }

        public Task SaveAsync<T>(IViewId viewId, T view) where T : IView
        {
            _outgoingCache.AddOrUpdate(viewId, view);
            _readCache.Set(new CacheItem(viewId.ToString(), view), new CacheItemPolicy {SlidingExpiration = _expirationPeriod});
            return Task.CompletedTask;
        }
    }
}