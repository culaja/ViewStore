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

        public long? ReadLastKnownPosition() => _viewStoreCacheInternal.ReadLastKnownPosition();

        public Task<long?> ReadLastKnownPositionAsync() => _viewStoreCacheInternal.ReadLastKnownPositionAsync();

        public T? Read<T>(string viewId) where T : IView => _viewStoreCacheInternal.Read<T>(viewId);

        public Task<T?> ReadAsync<T>(string viewId) where T : IView => _viewStoreCacheInternal.ReadAsync<T>(viewId);

        public void Save(IView view) => _viewStoreCacheInternal.Save(view);

        public Task SaveAsync(IView view) => _viewStoreCacheInternal.SaveAsync(view);

        public void Dispose()
        {
            _automaticCacheDrainer.Dispose();
        }
    }
}