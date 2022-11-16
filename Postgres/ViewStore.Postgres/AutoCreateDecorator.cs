using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using ViewStore.Abstractions;

namespace ViewStore.Postgres
{
    public delegate void PostCreationScriptProvider(NpgsqlConnection connection, NpgsqlTransaction transaction, string schemaName, string tableName);
    
    internal sealed class AutoCreateDecorator : IViewStore
    {
        private bool _isAutoCreated;
        
        private readonly string _connectionString;
        private readonly string _schemaName;
        private readonly string _tableName;
        private readonly AutoCreateOptions _autoCreateOptions;
        private readonly IViewStore _next;

        public AutoCreateDecorator(
            string connectionString,
            string schemaName,
            string tableName,
            AutoCreateOptions autoCreateOptions,
            IViewStore next)
        {
            _connectionString = connectionString;
            _schemaName = schemaName;
            _tableName = tableName;
            _autoCreateOptions = autoCreateOptions;
            _next = next;
        }
        
        public GlobalVersion? ReadLastGlobalVersion()
        {
            if (!_isAutoCreated)
            {
                Create(_connectionString, _schemaName, _tableName);
                _isAutoCreated = true;
            }

            return _next.ReadLastGlobalVersion();
        }

        public Task<GlobalVersion?> ReadLastGlobalVersionAsync()
        {
            if (!_isAutoCreated)
            {
                Create(_connectionString, _schemaName, _tableName);
                _isAutoCreated = true;
            }

            return _next.ReadLastGlobalVersionAsync();
        }

        public ViewEnvelope? Read(string viewId)
        {
            if (!_isAutoCreated)
            {
                Create(_connectionString, _schemaName, _tableName);
                _isAutoCreated = true;
            }

            return _next.Read(viewId);
        }

        public Task<ViewEnvelope?> ReadAsync(string viewId)
        {
            if (!_isAutoCreated)
            {
                Create(_connectionString, _schemaName, _tableName);
                _isAutoCreated = true;
            }

            return _next.ReadAsync(viewId);
        }

        public void Save(ViewEnvelope viewEnvelope)
        {
            if (!_isAutoCreated)
            {
                Create(_connectionString, _schemaName, _tableName);
                _isAutoCreated = true;
            }

            _next.Save(viewEnvelope);
        }

        public Task SaveAsync(ViewEnvelope viewEnvelope)
        {
            if (!_isAutoCreated)
            {
                Create(_connectionString, _schemaName, _tableName);
                _isAutoCreated = true;
            }

            return _next.SaveAsync(viewEnvelope);
        }

        public void Save(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            if (!_isAutoCreated)
            {
                Create(_connectionString, _schemaName, _tableName);
                _isAutoCreated = true;
            }
            
            _next.Save(viewEnvelopes);
        }

        public Task SaveAsync(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            if (!_isAutoCreated)
            {
                Create(_connectionString, _schemaName, _tableName);
                _isAutoCreated = true;
            }

            return _next.SaveAsync(viewEnvelopes);
        }

        public void Delete(string viewId, GlobalVersion globalVersion)
        {
            if (!_isAutoCreated)
            {
                Create(_connectionString, _schemaName, _tableName);
                _isAutoCreated = true;
            }
            
            _next.Delete(viewId, globalVersion);
        }

        public Task DeleteAsync(string viewId, GlobalVersion globalVersion)
        {
            if (!_isAutoCreated)
            {
                Create(_connectionString, _schemaName, _tableName);
                _isAutoCreated = true;
            }

            return _next.DeleteAsync(viewId, globalVersion);
        }

        public void Delete(IEnumerable<string> viewIds, GlobalVersion globalVersion)
        {
            if (!_isAutoCreated)
            {
                Create(_connectionString, _schemaName, _tableName);
                _isAutoCreated = true;
            }
            
            _next.Delete(viewIds, globalVersion);
        }

        public Task DeleteAsync(IEnumerable<string> viewIds, GlobalVersion globalVersion)
        {
            if (!_isAutoCreated)
            {
                Create(_connectionString, _schemaName, _tableName);
                _isAutoCreated = true;
            }

            return _next.DeleteAsync(viewIds, globalVersion);
        }
        
        private void Create(string connectionString, string schemaName, string tableName)
        {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            
            using var transaction = connection.BeginTransaction();
            if (_autoCreateOptions.ShouldCreateSchema) CreateSchema(connection, transaction, schemaName);
            if (_autoCreateOptions.ShouldCreateTable) CreateTable(connection, transaction, schemaName, tableName);
            _autoCreateOptions.PostCreationScriptProvider?.Invoke(connection, transaction, schemaName, tableName);
            transaction.Commit();
            
            connection.Close();
        }

        private void CreateSchema(NpgsqlConnection connection, NpgsqlTransaction transaction, string schemaName)
        {
            var sql = $"CREATE SCHEMA IF NOT EXISTS {schemaName}";
            using var cmd = new NpgsqlCommand(sql, connection, transaction);
            cmd.ExecuteNonQuery();
        }

        private void CreateTable(NpgsqlConnection connection, NpgsqlTransaction transaction, string schemaName, string tableName)
        {
            var sql = $@"
                CREATE TABLE IF NOT EXISTS {schemaName}.{tableName}(
                    id varchar(256) NOT NULL,
                    view jsonb NOT NULL,
                    viewType varchar(1024) NOT NULL,
                    shortViewType varchar(128) NOT NULL,
                    metadata jsonb NOT NULL,
                    globalVersion bigint NOT NULL,
                    lastChangeTimeStamp timestamp NOT NULL,
                    tenantId varchar(64) NOT NULL,
                    createdAt timestamp NOT NULL,
                    CONSTRAINT {schemaName}_{tableName}_pkey PRIMARY KEY (id,tenantId,createdAt));";
            
            using var cmd = new NpgsqlCommand(sql, connection, transaction);
            
            cmd.ExecuteNonQuery();
        }
    }
}