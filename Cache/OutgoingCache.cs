using System.Collections.Generic;
using System.Linq;
using ViewStore.Abstractions;

namespace ViewStore.Cache
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

        public IReadOnlyList<IView> Clear()
        {
            lock (_sync)
            {
                var cachedItems = _cache.Values.ToList();
                _cache = new Dictionary<string, IView>();
                return cachedItems;
            }   
        }
    }
}