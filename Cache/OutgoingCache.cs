using System.Collections.Generic;
using System.Linq;
using Abstractions;

namespace Cache
{
    public sealed class OutgoingCache<T> where T : IView
    {
        private object _sync = new();
        private Dictionary<string, T> _cache = new();

        public void AddOrUpdate(T view)
        {
            lock (_sync)
            {
                _cache[view.Id] = view;
            }
        }

        public bool TryGetValue(string viewId, out T view)
        {
            lock (_sync)
            {
                return _cache.TryGetValue(viewId, out view);
            }
        }

        public IReadOnlyList<T> Clear()
        {
            lock (_sync)
            {
                var cachedItems = _cache.Values.ToList();
                _cache = new Dictionary<string, T>();
                return cachedItems;
            }   
        }
    }
}