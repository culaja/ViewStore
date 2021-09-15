using System;
using Marten;
using ViewStore.Abstractions;
using Xunit;

namespace ViewStore.MartenDb
{
    internal sealed class MartenDbViewStoreTests : ViewStoreTests, IDisposable
    {
        private readonly IDocumentStore _documentStore;

        public MartenDbViewStoreTests()
        {
            _documentStore = DocumentStore.For(_ =>
            {
                _.AutoCreateSchemaObjects = AutoCreate.All;
                _.Connection("host=localhost;port=8276;database=EventStore;password=dagi123;username=root");
                _.Schema.For<ViewEnvelope>().DatabaseSchemaName($"S{Guid.NewGuid().ToString("N")}");
            });
        }

        protected override IViewStore BuildViewStore() => MartenDbViewStoreBuilder.New()
            .WithDocumentStore(_documentStore)
            .Build();

        public void Dispose()
        {
            _documentStore?.Dispose();
        }
    }
}