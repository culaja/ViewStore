using System.Collections.Generic;
using System.Linq;
using Abstractions;

namespace Cache
{
    public sealed class OutgoingCache
    {
        private object _sync = new();
        private Dictionary<IViewId, IView> _cache = new();

        public void AddOrUpdate(IViewId viewId, IView view)
        {
            lock (_sync)
            {
                _cache[viewId] = view;
            }
        }

        public bool TryGetValue(IViewId viewId, out IView view)
        {
            lock (_sync)
            {
                return _cache.TryGetValue(viewId, out view);
            }
        }

        public IReadOnlyList<KeyValuePair<IViewId, IView>> Clear()
        {
            lock (_sync)
            {
                var cachedItems = _cache.ToList();
                _cache = new Dictionary<IViewId, IView>();
                return cachedItems;
            }   
        }
    }
}