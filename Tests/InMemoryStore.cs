using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abstractions;

namespace Tests
{
    internal sealed class InMemoryStore : IViewStore
    {
        private readonly Dictionary<IViewId, IView> _dictionary = new();

        public long? ReadGlobalVersion<T>() where T: IView=> _dictionary.Values.Max(v => v.GlobalVersion);

        public Task<long?> ReadGlobalVersionAsync<T>() where T: IView => Task.FromResult(ReadGlobalVersion<T>());

        public T? Read<T>(IViewId viewId) where T : IView
        {
            if (_dictionary.TryGetValue(viewId, out var view))
            {
                return (T?)view;
            }

            return default;
        }

        public Task<T?> ReadAsync<T>(IViewId viewId) where T : IView => Task.FromResult(Read<T>(viewId));

        public void Save<T>(IViewId viewId, T view) where T : IView
        {
            _dictionary[viewId] = view;
        }

        public Task SaveAsync<T>(IViewId viewId, T view) where T : IView
        {
            Save(viewId, view);
            return Task.CompletedTask;
        }
    }
}