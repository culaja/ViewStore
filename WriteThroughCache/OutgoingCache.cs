using System.Collections.Generic;
using ViewStore.Abstractions;

namespace ViewStore.WriteThroughCache
{
    internal sealed class OutgoingCache
    {
        private readonly object _sync = new();
        private Dictionary<string, IView> _cache = new();

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
                return _cache.TryGetValue(viewId, out view);
            }
        }

        public IEnumerable<IView> Clear()
        {
            lock (_sync)
            {
                var cachedItems = _cache.Values;
                _cache = new Dictionary<string, IView>();
                return cachedItems;
            }  
        }
    }
}