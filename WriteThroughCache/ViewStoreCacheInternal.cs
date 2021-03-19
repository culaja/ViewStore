using System.Threading.Tasks;
using ViewStore.Abstractions;

namespace ViewStore.WriteThroughCache
{
    internal sealed class ViewStoreCacheInternal : IViewStore
    {
        private readonly IViewStore _next;
        private readonly OutgoingCache _outgoingCache;

        public ViewStoreCacheInternal(
            IViewStore next,
            OutgoingCache outgoingCache)
        {
            _next = next;
            _outgoingCache = outgoingCache;
        }

        public GlobalVersion? ReadLastKnownPosition() => _next.Read<ViewMetaData>(ViewMetaData.MetaDataId)?.GlobalVersion;

        public async Task<GlobalVersion?> ReadLastKnownPositionAsync()
        {
            var metadata = await  _next.ReadAsync<ViewMetaData>(ViewMetaData.MetaDataId);
            return metadata?.GlobalVersion;
        }

        public T? Read<T>(string viewId) where T : IView
        {
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
        }

        public Task SaveAsync(IView view)
        {
            Save(view);
            return Task.CompletedTask;
        }
    }
}