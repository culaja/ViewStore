using System.Collections.Generic;
using System.Threading.Tasks;
using Abstractions;

namespace Tests
{
    internal sealed class InMemoryStore : IViewStore
    {
        private readonly Dictionary<IViewId, IView> _dictionary = new();
        
        public Task<T?> ReadAsync<T>(IViewId viewId) where T : IView
        {
            if (_dictionary.TryGetValue(viewId, out var view))
            {
                return Task.FromResult((T?)view);
            }

            return Task.FromResult(default(T));
        }

        public Task SaveAsync<T>(IViewId viewId, T view) where T : IView
        {
            _dictionary[viewId] = view;
            return Task.CompletedTask;
        }
    }
}