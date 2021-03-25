using System.Collections.Generic;
using System.Linq;
using ViewStore.Abstractions;

namespace ViewStore.WriteBehindCache
{
    internal sealed class OutgoingCache
    {
        private readonly object _sync = new();
        private Dictionary<string, ViewEnvelope> _cache = new();
        private Dictionary<string, ViewEnvelope> _lastCache = new();

        public void AddOrUpdate(ViewEnvelope viewEnvelope)
        {
            lock (_sync)
            {
                _cache[viewEnvelope.Id] = viewEnvelope;
            }
        }

        public bool TryGetValue(string viewId, out ViewEnvelope viewEnvelope)
        {
            lock (_sync)
            {
                return _cache.TryGetValue(viewId, out viewEnvelope) || _lastCache.TryGetValue(viewId, out viewEnvelope);
            }
        }

        public GlobalVersion? LastGlobalVersion()
        {
            lock (_sync)
            {
                return _cache.Count > 0 || _lastCache.Count > 0 
                    ? _cache.Values.Concat(_lastCache.Values).Max(ve => ve.GlobalVersion)
                    : null;
            }
        }

        public IEnumerable<ViewEnvelope> Renew()
        {
            lock (_sync)
            {
                _lastCache = _cache;
                _cache = new Dictionary<string, ViewEnvelope>();
                return _lastCache.Values;
            }
        }
    }
}