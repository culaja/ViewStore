using System.Collections.Generic;
using System.Threading.Tasks;
using ViewStore.Abstractions;

namespace ViewStore.Cache
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

        public GlobalVersion? ReadLastGlobalVersion() =>
            GlobalVersion.Max(
                _outgoingCache.LastGlobalVersion(),
                ReadLastGlobalVersionFromNextStore());
        
        private GlobalVersion? ReadLastGlobalVersionFromNextStore()
        {
            var lastGlobalVersion = _next.Read(ViewMetaData.MetaDataId)?.GlobalVersion;
            if (lastGlobalVersion != null)
            {
                return lastGlobalVersion;
            }

            return _next.ReadLastGlobalVersion();
        }

        public async Task<GlobalVersion?> ReadLastGlobalVersionAsync() =>
            GlobalVersion.Max(
                _outgoingCache.LastGlobalVersion(),
                await ReadLastGlobalVersionFromNextStoreAsync());

        private async Task<GlobalVersion?> ReadLastGlobalVersionFromNextStoreAsync()
        {
            var lastGlobalVersion = (await _next.ReadAsync(ViewMetaData.MetaDataId))?.GlobalVersion;
            if (lastGlobalVersion != null)
            {
                return lastGlobalVersion;
            }

            return await _next.ReadLastGlobalVersionAsync();
        }

        public ViewEnvelope? Read(string viewId)
        {
            if (_outgoingCache.TryGetValue(viewId, out var view, out var isDeleted))
            {
                return view;
            }
            
            return isDeleted ? null : _next.Read(viewId);
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

        public void Save(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            _outgoingCache.AddOrUpdate(viewEnvelopes);
        }

        public Task SaveAsync(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            Save(viewEnvelopes);
            return Task.CompletedTask;
        }

        public void Delete(string viewId, GlobalVersion globalVersion)
        {
            _outgoingCache.Remove(viewId, globalVersion);
        }

        public Task DeleteAsync(string viewId, GlobalVersion globalVersion)
        {
            _outgoingCache.Remove(viewId, globalVersion);
            return Task.CompletedTask;
        }

        public void Delete(IEnumerable<string> viewIds, GlobalVersion globalVersion)
        {
            _outgoingCache.Remove(viewIds, globalVersion);
        }

        public Task DeleteAsync(IEnumerable<string> viewIds, GlobalVersion globalVersion)
        {
            _outgoingCache.Remove(viewIds, globalVersion);
            return Task.CompletedTask;
        }
    }
}