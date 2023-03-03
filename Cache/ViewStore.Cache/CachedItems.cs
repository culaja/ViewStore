using System.Collections.Generic;
using System.Linq;
using ViewStore.Abstractions;

namespace ViewStore.Cache
{
    internal sealed class CachedItems
    {
        private readonly Dictionary<string, ViewEnvelope> _addedOrUpdated = new();
        private readonly Dictionary<string, GlobalVersion> _deleted = new();

        public IReadOnlyCollection<ViewEnvelope> AddedOrUpdated => _addedOrUpdated.Values;

        public IReadOnlyCollection<DeletedViewEnvelope> Deleted => _deleted
            .Select(d => new DeletedViewEnvelope(d.Key, d.Value))
            .ToList();

        public long Count => _addedOrUpdated.Count + _deleted.Count;

        public void AddOrUpdate(ViewEnvelope viewEnvelope)
        {
            _deleted.Remove(viewEnvelope.Id);
            _addedOrUpdated[viewEnvelope.Id] = viewEnvelope;
        }
        
        public void AddOrUpdate(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            foreach (var viewEnvelope in viewEnvelopes)
            {
                _deleted.Remove(viewEnvelope.Id);
                _addedOrUpdated[viewEnvelope.Id] = viewEnvelope;    
            }
        }

        public void Remove(string viewId, GlobalVersion globalVersion)
        {
            _addedOrUpdated.Remove(viewId);
            _deleted[viewId] = globalVersion;
        }
        
        public void Remove(IEnumerable<string> viewIds, GlobalVersion globalVersion)
        {
            foreach (var viewId in viewIds)
            {
                _addedOrUpdated.Remove(viewId);
                _deleted[viewId] = globalVersion;    
            }
        }
        
        public bool TryGetValue(string viewId, out ViewEnvelope viewEnvelope, out bool isDeleted)
        {
            isDeleted = _deleted.ContainsKey(viewId);
            return _addedOrUpdated.TryGetValue(viewId, out viewEnvelope!);
        }

        public GlobalVersion? LastGlobalVersion() =>
            GlobalVersion.Max(
                _addedOrUpdated.Count > 0 ?_addedOrUpdated.Values.Max(ve => ve.GlobalVersion) : null,
                _deleted.Count > 0 ? _deleted.Values.Max() : null);
    }
}