using System;
using Marten;
using ViewStore.Abstractions;

namespace ViewStore.MartenDb
{
    public sealed class MartenDbViewStoreBuilder
    {
        private IDocumentStore? _documentStore;

        public static MartenDbViewStoreBuilder New() => new();

        public MartenDbViewStoreBuilder WithDocumentStore(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
            return this;
        }

        public IViewStore Build()
        {
            if (_documentStore == null) throw new ArgumentNullException(nameof(_documentStore), "Document store is not provided");
            
            return new MartenDbViewStore(_documentStore);
        }
    }
}