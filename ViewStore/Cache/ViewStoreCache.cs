using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ViewStore.Cache;

public sealed class ViewStoreCache : IViewStore, IDisposable
{
    private readonly ViewStoreCacheInternal _viewStoreCacheInternal;
    private readonly AutomaticCacheDrainer _automaticCacheDrainer;

    internal ViewStoreCache(ViewStoreCacheInternal viewStoreCacheInternal, AutomaticCacheDrainer automaticCacheDrainer)
    {
        _viewStoreCacheInternal = viewStoreCacheInternal;
        _automaticCacheDrainer = automaticCacheDrainer;
    }

    public Task<long?> ReadLastGlobalVersion() => _viewStoreCacheInternal.ReadLastGlobalVersion();
    public Task<ViewRecord?> Read(string viewId) => _viewStoreCacheInternal.Read(viewId);
    public void Save(ViewRecord viewRecord) => _viewStoreCacheInternal.Save(viewRecord);
    public void Save(IEnumerable<ViewRecord> viewRecords) => _viewStoreCacheInternal.Save(viewRecords);
    public void Delete(string viewId, long globalVersion = 0) => _viewStoreCacheInternal.Delete(viewId, globalVersion);
    public void Delete(IEnumerable<string> viewIds, long globalVersion) => _viewStoreCacheInternal.Delete(viewIds, globalVersion);
        
    public void Dispose()
    {
        _automaticCacheDrainer.Dispose();
    }
}