﻿using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;
using ViewStore.Abstractions;

namespace ViewStore.Cache
{
    internal sealed class ReadThroughViewStoreFlusherCache : IViewStoreFlusher
    {
        private readonly MemoryCache _memoryCache;
        private readonly TimeSpan _readCacheExpirationPeriod;
        private readonly IViewStoreFlusher _next;

        public ReadThroughViewStoreFlusherCache(
            MemoryCache memoryCache,
            TimeSpan readCacheExpirationPeriod,
            IViewStoreFlusher next)
        {
            _memoryCache = memoryCache;
            _readCacheExpirationPeriod = readCacheExpirationPeriod;
            _next = next;
        }

        public Task<long?> ReadLastGlobalVersionAsync() => _next.ReadLastGlobalVersionAsync();

        public Task SaveLastGlobalVersionAsync(long globalVersion) => _next.SaveLastGlobalVersionAsync(globalVersion);

        public async Task<ViewRecord?> ReadAsync(string viewId)
        {
            CacheItem? optionalCacheItem = _memoryCache.GetCacheItem(viewId);
            if (optionalCacheItem != null)
            {
                return (ViewRecord)optionalCacheItem.Value;
            }
        
            var viewEnvelope = await _next.ReadAsync(viewId);
            if (viewEnvelope != null)
            {
                _memoryCache.Set(new CacheItem(viewId, viewEnvelope), new CacheItemPolicy {SlidingExpiration = _readCacheExpirationPeriod});
            }
        
            return viewEnvelope;
        }

        public Task<long> UpsertAsync(IEnumerable<ViewRecord> viewRecords)
        {
            var list = new List<ViewRecord>();
            foreach (var viewEnvelope in viewRecords)
            {
                _memoryCache.Set(new CacheItem(viewEnvelope.Id, viewEnvelope), new CacheItemPolicy {SlidingExpiration = _readCacheExpirationPeriod});
                list.Add(viewEnvelope);
            }
            
            return _next.UpsertAsync(list);
        }

        public Task<long> DeleteAsync(IEnumerable<string> viewIds)
        {
            var list = new List<string>();
            foreach (var viewId in viewIds)
            {
                _memoryCache.Remove(viewId);
                list.Add(viewId);
            }
            
            return _next.DeleteAsync(list);
        }
    }
}