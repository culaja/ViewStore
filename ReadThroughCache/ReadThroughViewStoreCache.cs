using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using ViewStore.Abstractions;

namespace ViewStore.ReadThroughCache
{
    internal sealed class ReadThroughViewStoreCache : IViewStore
    {
        private readonly MemoryCache _memoryCache;
        private readonly TimeSpan _readCacheExpirationPeriod;
        private readonly IViewStore _next;

        public ReadThroughViewStoreCache(
            MemoryCache memoryCache,
            TimeSpan readCacheExpirationPeriod,
            IViewStore next)
        {
            _memoryCache = memoryCache;
            _readCacheExpirationPeriod = readCacheExpirationPeriod;
            _next = next;
        }

        public long? ReadLastKnownPosition() => _next.ReadLastKnownPosition();

        public Task<long?> ReadLastKnownPositionAsync() => _next.ReadLastKnownPositionAsync();

        public T? Read<T>(string viewId) where T : IView
        {
            CacheItem? optionalCacheItem = _memoryCache.GetCacheItem(viewId);
            if (optionalCacheItem != null)
            {
                return (T)optionalCacheItem.Value;
            }

            var optionalView = _next.Read<T>(viewId);
            if (optionalView != null)
            {
                _memoryCache.Set(new CacheItem(viewId, optionalView), new CacheItemPolicy {SlidingExpiration = _readCacheExpirationPeriod});
            }

            return optionalView;
        }

        public async Task<T?> ReadAsync<T>(string viewId) where T : IView
        {
            CacheItem? optionalCacheItem = _memoryCache.GetCacheItem(viewId);
            if (optionalCacheItem != null)
            {
                return (T)optionalCacheItem.Value;
            }

            var optionalView = await _next.ReadAsync<T>(viewId);
            if (optionalView != null)
            {
                _memoryCache.Set(new CacheItem(viewId, optionalView), new CacheItemPolicy {SlidingExpiration = _readCacheExpirationPeriod});
            }

            return optionalView;
        }

        public void Save(IView view)
        {
            _memoryCache.Set(new CacheItem(view.Id, view), new CacheItemPolicy {SlidingExpiration = _readCacheExpirationPeriod});
            _next.Save(view);
        }

        public Task SaveAsync(IView view)
        {
            _memoryCache.Set(new CacheItem(view.Id, view), new CacheItemPolicy {SlidingExpiration = _readCacheExpirationPeriod});
            return _next.SaveAsync(view);
        }
    }
}