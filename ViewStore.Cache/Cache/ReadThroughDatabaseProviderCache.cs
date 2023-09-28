﻿using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace ViewStore.Cache.Cache;

internal sealed class ReadThroughDatabaseProviderCache : IDatabaseProvider
{
    private readonly MemoryCache _memoryCache;
    private readonly TimeSpan _readCacheExpirationPeriod;
    private readonly IDatabaseProvider _databaseProvider;

    public ReadThroughDatabaseProviderCache(
        MemoryCache memoryCache,
        TimeSpan readCacheExpirationPeriod,
        IDatabaseProvider databaseProvider)
    {
        _memoryCache = memoryCache;
        _readCacheExpirationPeriod = readCacheExpirationPeriod;
        _databaseProvider = databaseProvider;
    }

    public Task PrepareSchema() => _databaseProvider.PrepareSchema();

    public Task<long?> ReadLastGlobalVersionAsync() => _databaseProvider.ReadLastGlobalVersionAsync();

    public Task SaveLastGlobalVersionAsync(long globalVersion) => _databaseProvider.SaveLastGlobalVersionAsync(globalVersion);

    public async Task<ViewRecord?> ReadAsync(string viewId)
    {
        CacheItem? optionalCacheItem = _memoryCache.GetCacheItem(viewId);
        if (optionalCacheItem != null)
        {
            return (ViewRecord)optionalCacheItem.Value;
        }
        
        var viewEnvelope = await _databaseProvider.ReadAsync(viewId);
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
            
        return _databaseProvider.UpsertAsync(list);
    }

    public Task<long> DeleteAsync(IEnumerable<string> viewIds)
    {
        var list = new List<string>();
        foreach (var viewId in viewIds)
        {
            _memoryCache.Remove(viewId);
            list.Add(viewId);
        }
            
        return _databaseProvider.DeleteAsync(list);
    }
}