using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViewStore.Abstractions;

namespace ViewStore.WriteBehindCache
{
    public sealed class ViewStoreCache : IViewStore, IDisposable
    {
        private readonly ViewStoreCacheInternal _viewStoreCacheInternal;
        private readonly AutomaticCacheDrainer _automaticCacheDrainer;

        internal ViewStoreCache(ViewStoreCacheInternal viewStoreCacheInternal, AutomaticCacheDrainer automaticCacheDrainer)
        {
            _viewStoreCacheInternal = viewStoreCacheInternal;
            _automaticCacheDrainer = automaticCacheDrainer;
        }

        public GlobalVersion? ReadLastGlobalVersion() => _viewStoreCacheInternal.ReadLastGlobalVersion();

        public Task<GlobalVersion?> ReadLastGlobalVersionAsync() => _viewStoreCacheInternal.ReadLastGlobalVersionAsync();

        public ViewEnvelope? Read(string viewId) => _viewStoreCacheInternal.Read(viewId);

        public Task<ViewEnvelope?> ReadAsync(string viewId) => _viewStoreCacheInternal.ReadAsync(viewId);

        public void Save(ViewEnvelope viewEnvelope) => _viewStoreCacheInternal.Save(viewEnvelope);

        public Task SaveAsync(ViewEnvelope viewEnvelope) => _viewStoreCacheInternal.SaveAsync(viewEnvelope);
        
        public void Save(IEnumerable<ViewEnvelope> viewEnvelopes) => _viewStoreCacheInternal.Save(viewEnvelopes);

        public Task SaveAsync(IEnumerable<ViewEnvelope> viewEnvelopes) => _viewStoreCacheInternal.SaveAsync(viewEnvelopes);
        public void Delete(ViewEnvelope viewEnvelope) => _viewStoreCacheInternal.Delete(viewEnvelope);

        public Task DeleteAsync(ViewEnvelope viewEnvelope) => _viewStoreCacheInternal.DeleteAsync(viewEnvelope);
        
        public void Delete(IEnumerable<ViewEnvelope> viewEnvelopes) => _viewStoreCacheInternal.Delete(viewEnvelopes);
        
        public Task DeleteAsync(IEnumerable<ViewEnvelope> viewEnvelopes) => _viewStoreCacheInternal.DeleteAsync(viewEnvelopes);

        public void Dispose()
        {
            _automaticCacheDrainer.Dispose();
        }
    }
}