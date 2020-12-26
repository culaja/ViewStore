using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Abstractions;

namespace Cache
{
    internal sealed class ViewStoreCache : IViewStore
    {
        private readonly IViewStore _next;
        private readonly MemoryCache _readCache;
        private readonly OutgoingCache _outgoingCache;
        private readonly TimeSpan _expirationPeriod;

        public ViewStoreCache(
            IViewStore next,
            MemoryCache readCache,
            OutgoingCache outgoingCache,
            TimeSpan expirationPeriod)
        {
            _next = next;
            _readCache = readCache;
            _outgoingCache = outgoingCache;
            _expirationPeriod = expirationPeriod;
        }

        public long? ReadLastKnownPosition() => _next.Read<ViewMetaData>(ViewMetaData.MetaDataId)?.GlobalVersion;

        public async Task<long?> ReadLastKnownPositionAsync()
        {
            var metadata = await  _next.ReadAsync<ViewMetaData>(ViewMetaData.MetaDataId);
            return metadata?.GlobalVersion;
        }

        public T? Read<T>(string viewId) where T : IView
        {
            CacheItem? optionalCacheItem = _readCache.GetCacheItem(viewId);
            if (optionalCacheItem != null)
            {
                return (T)optionalCacheItem.Value;
            }
            
            if (_outgoingCache.TryGetValue(viewId, out var view))
            {
                return (T)view;
            }
            
            return _next.Read<T>(viewId);
        }

        public Task<T?> ReadAsync<T>(string viewId) where T : IView  => Task.FromResult(Read<T>(viewId));

        public void Save(IView view)
        {
            _outgoingCache.AddOrUpdate(view);
            _readCache.Set(new CacheItem(view.Id, view), new CacheItemPolicy {SlidingExpiration = _expirationPeriod});
        }

        public Task SaveAsync(IView view)
        {
            Save(view);
            return Task.CompletedTask;
        }
    }
}