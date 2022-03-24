using System;
using System.Threading.Tasks;
using Npgsql;
using ViewStore.Abstractions;

namespace ViewStore.Postgres
{
    public sealed class PostgresViewStoreBuilder
    {
        private string? _connectionString;
        private string _schemaName = "public";
        private string? _tableName;
        private bool _shouldAutoCreate;
        private  Func<NpgsqlConnection, NpgsqlTransaction, Task> _postAutoCreationCustomization = (_,_) => Task.CompletedTask;
        
        public static PostgresViewStoreBuilder New() => new();
        
        public PostgresViewStoreBuilder WithConnectionString(string connectionString)
        {
            _connectionString = connectionString;
            return this;
        }
        
        public PostgresViewStoreBuilder WithSchemaName(string schemaName)
        {
            _schemaName = schemaName;
            return this;
        }
        
        public PostgresViewStoreBuilder WithTableName(string tableName)
        {
            _tableName = tableName;
            return this;
        }

        public PostgresViewStoreBuilder WithTablePath(string tablePath)
        {
            var splitTablePath = tablePath.Split(".");
            if (splitTablePath.Length != 2) throw new ArgumentException("Table path should be in <schemaName>.<tableName> format", nameof(tablePath));
            _schemaName = splitTablePath[0];
            _tableName = splitTablePath[1];
            return this;
        }
        
        public PostgresViewStoreBuilder ShouldAutoCreate(bool shouldAutoCreate)
        {
            _shouldAutoCreate = shouldAutoCreate;
            return this;
        }

        public PostgresViewStoreBuilder UsePostAutoCreationCustomization(Func<NpgsqlConnection, NpgsqlTransaction, Task> postAutoCreationCustomization)
        {
            _postAutoCreationCustomization = postAutoCreationCustomization;
            return this;
        }
        
        public IViewStore Build()
        {
            if (_connectionString == null) throw new ArgumentNullException(nameof(_connectionString), "Connection string is not provided");
            if (_tableName == null) throw new ArgumentNullException(nameof(_tableName), "Table name is not provided");
            
            var viewStore = new PostgresViewStore(
                _connectionString,
                _schemaName,
                _tableName);

            return _shouldAutoCreate
                ? new AutoCreateDecorator(
                    _connectionString,
                    _schemaName,
                    _tableName,
                    _postAutoCreationCustomization,
                    viewStore)
                : viewStore;
        }
    }
}