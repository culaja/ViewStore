using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViewStore.Abstractions;

namespace ViewStore.Tests
{
    internal sealed class InMemoryStore : IViewStore
    {
        private readonly Dictionary<string, IView> _dictionary = new();

        public GlobalVersion? ReadLastKnownPosition() => _dictionary.Count > 0
            ? _dictionary.Values.Max(v => v.GlobalVersion)
            : default;

        public Task<GlobalVersion?> ReadLastKnownPositionAsync() => Task.FromResult(ReadLastKnownPosition());

        public T? Read<T>(string viewId) where T : IView
        {
            if (_dictionary.TryGetValue(viewId, out var view))
            {
                return (T) view;
            }

            return default;
        }

        public Task<T?> ReadAsync<T>(string viewId) where T : IView => Task.FromResult(Read<T>(viewId));

        public void Save(IView view)
        {
            _dictionary[view.Id] = view;
        }

        public Task SaveAsync(IView view)
        {
            Save(view);
            return Task.CompletedTask;
        }
    }
}