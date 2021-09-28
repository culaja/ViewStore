using System.Collections.Generic;
using System.Linq;
using ViewStore.Abstractions;

namespace ViewStore.WriteBehindCache
{
    internal sealed class OutgoingCache
    {
        private readonly object _sync = new();
        private Dictionary<string, ViewEnvelope> _currentCache = new();
        private Dictionary<string, ViewEnvelope> _drainedCache = new();

        public void AddOrUpdate(ViewEnvelope viewEnvelope)
        {
            lock (_sync)
            {
                _currentCache[viewEnvelope.Id] = viewEnvelope;
            }
        }

        public bool Remove(string id)
        {
            lock (_sync)
            {
                return _currentCache.Remove(id);
            }
        }

        public bool TryGetValue(string viewId, out ViewEnvelope viewEnvelope)
        {
            lock (_sync)
            {
                return _currentCache.TryGetValue(viewId, out viewEnvelope) || _drainedCache.TryGetValue(viewId, out viewEnvelope);
            }
        }

        public GlobalVersion? LastGlobalVersion()
        {
            lock (_sync)
            {
                return _currentCache.Count > 0 || _drainedCache.Count > 0 
                    ? _currentCache.Values.Concat(_drainedCache.Values).Max(ve => ve.GlobalVersion)
                    : null;
            }
        }

        public IEnumerable<ViewEnvelope> Renew()
        {
            lock (_sync)
            {
                _drainedCache = _currentCache;
                _currentCache = new Dictionary<string, ViewEnvelope>();
                return _drainedCache.Values;
            }
        }
    }
}