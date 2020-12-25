using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abstractions;

namespace Tests
{
    internal sealed class InMemoryStore<T> : IViewStore<T> where T : IView
    {
        private readonly Dictionary<string, T> _dictionary = new();

        public long? ReadGlobalVersion() => _dictionary.Values.Max(v => v.GlobalVersion);

        public Task<long?> ReadGlobalVersionAsync() => Task.FromResult(ReadGlobalVersion());

        public T? Read(string viewId)
        {
            if (_dictionary.TryGetValue(viewId, out var view))
            {
                return (T?)view;
            }

            return default;
        }

        public Task<T?> ReadAsync(string viewId) => Task.FromResult(Read(viewId));

        public void Save(T view)
        {
            _dictionary[view.Id] = view;
        }

        public Task SaveAsync(T view)
        {
            Save(view);
            return Task.CompletedTask;
        }
    }
}