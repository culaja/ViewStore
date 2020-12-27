using System.Collections.Generic;
using ViewStore.Abstractions;

namespace ViewStore.WriteThroughCache
{
    internal sealed class OutgoingCache
    {
        private readonly object _sync = new();
        private Dictionary<string, IView> _cache = new();
        private Dictionary<string, IView> _lastCache = new();

        public void AddOrUpdate(IView view)
        {
            lock (_sync)
            {
                _cache[view.Id] = view;
            }
        }

        public bool TryGetValue(string viewId, out IView view)
        {
            lock (_sync)
            {
                return _cache.TryGetValue(viewId, out view) || _lastCache.TryGetValue(viewId, out view);
            }
        }

        public IEnumerable<IView> Renew()
        {
            lock (_sync)
            {
                _lastCache = _cache;
                _cache = new Dictionary<string, IView>();
                return _lastCache.Values;
            }
        }
    }
}