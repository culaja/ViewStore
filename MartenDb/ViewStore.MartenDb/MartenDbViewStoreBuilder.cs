using System;
using Marten;
using ViewStore.Abstractions;
using Weasel.Postgresql;

namespace ViewStore.MartenDb
{
    public sealed class MartenDbViewStoreBuilder
    {
        private string? _connectionString;
        private string? _schemaName;
        private AutoCreate _autoCreate = AutoCreate.None;

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

        public MartenDbViewStoreBuilder WithAutoCreate(AutoCreate autoCreate)
        {
            _autoCreate = autoCreate;
            return this;
        }

        public IViewStore Build()
        {
            if (_connectionString == null) throw new ArgumentNullException(nameof(_connectionString), "Connection string is not provided");
            if (_schemaName == null) throw new ArgumentNullException(nameof(_schemaName), "Schema name is not provided");
            
            var documentStore = DocumentStore.For(_ =>
            {
                _.AutoCreateSchemaObjects = _autoCreate;
                _.Connection(_connectionString);
                _.Schema.For<ViewEnvelopeInternal>().DatabaseSchemaName(_schemaName);
                _.Schema.For<ViewEnvelopeInternal>().Identity(x => x.Id);
                _.Schema.For<ViewEnvelopeInternal>().Index(ve => ve.Id);
                _.Schema.For<ViewEnvelopeInternal>().Index(ve => ve.GlobalVersion);
            });
            
            return new MartenDbViewStore(documentStore);
        }
    }
}