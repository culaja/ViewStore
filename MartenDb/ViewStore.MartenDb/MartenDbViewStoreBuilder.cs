using System;
using Marten;
using ViewStore.Abstractions;

namespace ViewStore.MartenDb
{
    public sealed class MartenDbViewStoreBuilder
    {
        private string? _connectionString;
        private string? _schemaName;

        public static MartenDbViewStoreBuilder New() => new();

        public MartenDbViewStoreBuilder WithConnectionString(string connectionString)
        {
            _connectionString = connectionString;
            return this;
        }
        
        public MartenDbViewStoreBuilder WithSchemaName(string schemaName)
        {
            _schemaName = schemaName;
            return this;
        }

        public IViewStore Build()
        {
            if (_connectionString == null) throw new ArgumentNullException(nameof(_connectionString), "Connection string is not provided");
            if (_schemaName == null) throw new ArgumentNullException(nameof(_schemaName), "Schema name is not provided");
            
            var documentStore = DocumentStore.For(_ =>
            {
                _.AutoCreateSchemaObjects = AutoCreate.All;
                _.Connection(_connectionString);
                _.Schema.For<ViewEnvelope>().DatabaseSchemaName(_schemaName);
                _.Schema.For<ViewEnvelope>().Identity(x => x.Id);
                _.Schema.For<ViewEnvelope>().Index(ve => ve.Id);
                _.Schema.For<ViewEnvelope>().Index(ve => ve.GlobalVersion);
            });
            
            return new MartenDbViewStore(documentStore);
        }
    }
}