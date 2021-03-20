using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViewStore.Abstractions;

namespace ViewStore.Tests
{
    internal sealed class InMemoryStore : IViewStore
    {
        private readonly Dictionary<string, ViewEnvelope> _dictionary = new();

        public GlobalVersion? ReadLastKnownPosition() => _dictionary.Count > 0
            ? _dictionary.Values.Max(v => v.GlobalVersion)
            : default;

        public Task<GlobalVersion?> ReadLastKnownPositionAsync() => Task.FromResult(ReadLastKnownPosition());

        public ViewEnvelope? Read(string viewId)
        {
            if (_dictionary.TryGetValue(viewId, out var view))
            {
                return view;
            }

            return default;
        }

        public Task<ViewEnvelope?> ReadAsync(string viewId) => Task.FromResult(Read(viewId));

        public void Save(ViewEnvelope viewEnvelope)
        {
            _dictionary[viewEnvelope.Id] = viewEnvelope;
        }

        public Task SaveAsync(ViewEnvelope viewEnvelope)
        {
            Save(viewEnvelope);
            return Task.CompletedTask;
        }
    }
}