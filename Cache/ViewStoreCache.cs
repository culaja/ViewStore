using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Abstractions;

namespace Cache
{
    public sealed class ViewStoreCache<T> : IViewStore<T> where T : IView
    {
        private readonly IViewStore<T> _next;
        private readonly MemoryCache _readCache = MemoryCache.Default;
        private readonly OutgoingCache<T> _outgoingCache;
        private readonly TimeSpan _expirationPeriod;

        public ViewStoreCache(
            IViewStore<T> next,
            OutgoingCache<T> outgoingCache,
            TimeSpan expirationPeriod)
        {
            _next = next;
            _outgoingCache = outgoingCache;
            _expirationPeriod = expirationPeriod;
        }

        public T? Read(string viewId)
        {
            CacheItem? optionalCacheItem = _readCache.GetCacheItem(viewId);
            if (optionalCacheItem != null)
            {
                return (T?) optionalCacheItem.Value;
            }
            
            if (_outgoingCache.TryGetValue(viewId, out var view))
            {
                return (T?) view;
            }
            
            return _next.Read(viewId);
        }

        public Task<T?> ReadAsync(string viewId) => Task.FromResult(Read(viewId));

        public void Save(T view)
        {
            _outgoingCache.AddOrUpdate(view);
            _readCache.Set(new CacheItem(view.Id, view), new CacheItemPolicy {SlidingExpiration = _expirationPeriod});
        }

        public Task SaveAsync(T view)
        {
            Save(view);
            return Task.CompletedTask;
        }
    }
}