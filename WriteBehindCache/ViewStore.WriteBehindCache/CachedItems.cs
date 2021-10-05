using System.Collections.Generic;
using System.Linq;
using ViewStore.Abstractions;

namespace ViewStore.WriteBehindCache
{
    internal sealed class CachedItems
    {
        private readonly Dictionary<string, ViewEnvelope> _addedOrUpdated = new();
        private readonly Dictionary<string, ViewEnvelope> _deleted = new();

        public IReadOnlyCollection<ViewEnvelope> AddedOrUpdated => _addedOrUpdated.Values;
        public IReadOnlyCollection<ViewEnvelope> Deleted => _deleted.Values;
        
        public void AddOrUpdate(ViewEnvelope viewEnvelope)
        {
            _deleted.Remove(viewEnvelope.Id);
            _addedOrUpdated[viewEnvelope.Id] = viewEnvelope;
        }

        public void Remove(ViewEnvelope viewEnvelope)
        {
            _addedOrUpdated.Remove(viewEnvelope.Id);
            _deleted[viewEnvelope.Id] = viewEnvelope;
        }
        
        public bool TryGetValue(string viewId, out ViewEnvelope viewEnvelope, out bool isDeleted)
        {
            isDeleted = _deleted.ContainsKey(viewId);
            return _addedOrUpdated.TryGetValue(viewId, out viewEnvelope);
        }

        public GlobalVersion? LastGlobalVersion() =>
            _addedOrUpdated.Count > 0 || _deleted.Count > 0 
                ? _addedOrUpdated.Values.Concat(_deleted.Values).Max(ve => ve.GlobalVersion)
                : null;
    }
}