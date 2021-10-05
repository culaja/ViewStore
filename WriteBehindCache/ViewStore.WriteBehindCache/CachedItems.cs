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
        public IReadOnlyCollection<ViewEnvelope> Deleted => _addedOrUpdated.Values;
        
        public void AddOrUpdate(ViewEnvelope viewEnvelope)
        {
            _addedOrUpdated[viewEnvelope.Id] = viewEnvelope;
        }

        public void Remove(ViewEnvelope viewEnvelope)
        {
            _deleted[viewEnvelope.Id] = viewEnvelope;
        }
        
        public bool TryGetValue(string viewId, out ViewEnvelope viewEnvelope) => 
            _addedOrUpdated.TryGetValue(viewId, out viewEnvelope);
        
        public GlobalVersion? LastGlobalVersion() =>
            _addedOrUpdated.Count > 0 || _deleted.Count > 0 
                ? _addedOrUpdated.Values.Concat(_deleted.Values).Max(ve => ve.GlobalVersion)
                : null;
    }
}