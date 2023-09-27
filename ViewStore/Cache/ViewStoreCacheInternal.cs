using System.Collections.Generic;
using System.Threading.Tasks;

namespace ViewStore.Cache
{
    internal sealed class ViewStoreCacheInternal : IViewStore
    {
        private readonly IDatabaseProvider _databaseProvider;
        private readonly OutgoingCache _outgoingCache;

        public ViewStoreCacheInternal(
            IDatabaseProvider databaseProvider,
            OutgoingCache outgoingCache)
        {
            _databaseProvider = databaseProvider;
            _outgoingCache = outgoingCache;
        }

        public Task<long?> ReadLastGlobalVersion()
        {
            if (_outgoingCache.LastGlobalVersion() is { } lastGlobalVersion)
            {
                return Task.FromResult<long?>(lastGlobalVersion);
            }

            return _databaseProvider.ReadLastGlobalVersionAsync();
        }

        public async Task<ViewRecord?> Read(string viewId)
        {
            if (_outgoingCache.TryGetValue(viewId, out var view, out var isDeleted))
            {
                return view;
            }
            
            return isDeleted ? null : await _databaseProvider.ReadAsync(viewId);
        }

        public void Save(ViewRecord viewRecord)
        {
            _outgoingCache.AddOrUpdate(viewRecord);
        }

        public void Save(IEnumerable<ViewRecord> viewRecords)
        {
            _outgoingCache.AddOrUpdate(viewRecords);
        }

        public void Delete(string viewId, long globalVersion = 0)
        {
            _outgoingCache.Remove(viewId, globalVersion);
        }

        public void Delete(IEnumerable<string> viewIds, long globalVersion = 0)
        {
            _outgoingCache.Remove(viewIds, globalVersion);
        }
    }
}