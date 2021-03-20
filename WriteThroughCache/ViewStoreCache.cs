using System;
using System.Threading.Tasks;
using ViewStore.Abstractions;

namespace ViewStore.WriteThroughCache
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

        public GlobalVersion? ReadLastKnownPosition() => _viewStoreCacheInternal.ReadLastKnownPosition();

        public Task<GlobalVersion?> ReadLastKnownPositionAsync() => _viewStoreCacheInternal.ReadLastKnownPositionAsync();

        public ViewEnvelope? Read(string viewId) => _viewStoreCacheInternal.Read(viewId);

        public Task<ViewEnvelope?> ReadAsync(string viewId) => _viewStoreCacheInternal.ReadAsync(viewId);

        public void Save(ViewEnvelope viewEnvelope) => _viewStoreCacheInternal.Save(viewEnvelope);

        public Task SaveAsync(ViewEnvelope viewEnvelope) => _viewStoreCacheInternal.SaveAsync(viewEnvelope);

        public void Dispose()
        {
            _automaticCacheDrainer.Dispose();
        }
    }
}