using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;
using ViewStore.Abstractions;

namespace ViewStore.ReadThroughCache
{
    public sealed class ReadThroughViewStoreCache : IViewStore
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

        public GlobalVersion? ReadLastGlobalVersion() => _next.ReadLastGlobalVersion();

        public Task<GlobalVersion?> ReadLastGlobalVersionAsync() => _next.ReadLastGlobalVersionAsync();

        public ViewEnvelope? Read(string viewId)
        {
            CacheItem? optionalCacheItem = _memoryCache.GetCacheItem(viewId);
            if (optionalCacheItem != null)
            {
                return (ViewEnvelope)optionalCacheItem.Value;
            }

            var viewEnvelope = _next.Read(viewId);
            if (viewEnvelope != null)
            {
                _memoryCache.Set(new CacheItem(viewId, viewEnvelope), new CacheItemPolicy {SlidingExpiration = _readCacheExpirationPeriod});
            }

            return viewEnvelope;
        }

        public async Task<ViewEnvelope?> ReadAsync(string viewId)
        {
            CacheItem? optionalCacheItem = _memoryCache.GetCacheItem(viewId);
            if (optionalCacheItem != null)
            {
                return (ViewEnvelope)optionalCacheItem.Value;
            }

            var viewEnvelope = await _next.ReadAsync(viewId);
            if (viewEnvelope != null)
            {
                _memoryCache.Set(new CacheItem(viewId, viewEnvelope), new CacheItemPolicy {SlidingExpiration = _readCacheExpirationPeriod});
            }

            return viewEnvelope;
        }

        public void Save(ViewEnvelope viewEnvelope)
        {
            _memoryCache.Set(new CacheItem(viewEnvelope.Id, viewEnvelope), new CacheItemPolicy {SlidingExpiration = _readCacheExpirationPeriod});
            _next.Save(viewEnvelope);
        }

        public Task SaveAsync(ViewEnvelope viewEnvelope)
        {
            _memoryCache.Set(new CacheItem(viewEnvelope.Id, viewEnvelope), new CacheItemPolicy {SlidingExpiration = _readCacheExpirationPeriod});
            return _next.SaveAsync(viewEnvelope);
        }

        public void Save(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            var list = new List<ViewEnvelope>();
            foreach (var viewEnvelope in viewEnvelopes)
            {
                _memoryCache.Set(new CacheItem(viewEnvelope.Id, viewEnvelope), new CacheItemPolicy {SlidingExpiration = _readCacheExpirationPeriod});
                list.Add(viewEnvelope);
            }
            
            _next.Save(list);
        }

        public Task SaveAsync(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            var list = new List<ViewEnvelope>();
            foreach (var viewEnvelope in viewEnvelopes)
            {
                _memoryCache.Set(new CacheItem(viewEnvelope.Id, viewEnvelope), new CacheItemPolicy {SlidingExpiration = _readCacheExpirationPeriod});
                list.Add(viewEnvelope);
            }
            
            return _next.SaveAsync(list);
        }

        public void Delete(ViewEnvelope viewEnvelope)
        {
            _memoryCache.Remove(viewEnvelope.Id);
            _next.Delete(viewEnvelope);
        }

        public Task DeleteAsync(ViewEnvelope viewEnvelope)
        {
            _memoryCache.Remove(viewEnvelope.Id);
            return _next.DeleteAsync(viewEnvelope);
        }

        public void Delete(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            var list = new List<ViewEnvelope>();
            foreach (var viewEnvelope in viewEnvelopes)
            {
                _memoryCache.Remove(viewEnvelope.Id);
                list.Add(viewEnvelope);
            }
            
            _next.Delete(list);
        }

        public Task DeleteAsync(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            var list = new List<ViewEnvelope>();
            foreach (var viewEnvelope in viewEnvelopes)
            {
                _memoryCache.Remove(viewEnvelope.Id);
                list.Add(viewEnvelope);
            }
            
            return _next.DeleteAsync(list);
        }
    }
}