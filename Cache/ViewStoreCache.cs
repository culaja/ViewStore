using System;
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

        public long? ReadGlobalVersion<T>()  where T : IView => _next.ReadGlobalVersion<T>();

        public Task<long?> ReadGlobalVersionAsync<T>() where T : IView => _next.ReadGlobalVersionAsync<T>();

        public T? Read<T>(IViewId viewId) where T : IView
        {
            CacheItem? optionalCacheItem = _readCache.GetCacheItem(viewId.ToString());
            if (optionalCacheItem != null)
            {
                return (T?) optionalCacheItem.Value;
            }
            
            if (_outgoingCache.TryGetValue(viewId, out var view))
            {
                return (T?) view;
            }
            
            return _next.Read<T>(viewId);
        }

        public Task<T?> ReadAsync<T>(IViewId viewId) where T : IView => Task.FromResult(Read<T>(viewId));

        public void Save<T>(IViewId viewId, T view) where T : IView
        {
            _outgoingCache.AddOrUpdate(viewId, view);
            _readCache.Set(new CacheItem(viewId.ToString(), view), new CacheItemPolicy {SlidingExpiration = _expirationPeriod});
        }

        public Task SaveAsync<T>(IViewId viewId, T view) where T : IView
        {
            Save(viewId, view);
            return Task.CompletedTask;
        }
    }
}