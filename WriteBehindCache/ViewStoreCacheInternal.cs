using System.Threading.Tasks;
using ViewStore.Abstractions;

namespace ViewStore.WriteBehindCache
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

        public GlobalVersion? ReadLastKnownPosition() => _next.Read(ViewMetaData.MetaDataId)?.GlobalVersion;

        public async Task<GlobalVersion?> ReadLastKnownPositionAsync()
        {
            var metadata = await  _next.ReadAsync(ViewMetaData.MetaDataId);
            return metadata?.GlobalVersion;
        }

        public ViewEnvelope? Read(string viewId)
        {
            if (_outgoingCache.TryGetValue(viewId, out var view))
            {
                return view;
            }
            
            return _next.Read(viewId);
        }

        public Task<ViewEnvelope?> ReadAsync(string viewId) => Task.FromResult(Read(viewId));

        public void Save(ViewEnvelope viewEnvelope)
        {
            _outgoingCache.AddOrUpdate(viewEnvelope);
        }

        public Task SaveAsync(ViewEnvelope viewEnvelope)
        {
            Save(viewEnvelope);
            return Task.CompletedTask;
        }
    }
}