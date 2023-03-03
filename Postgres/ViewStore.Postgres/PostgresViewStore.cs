using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using ViewStore.Abstractions;

namespace ViewStore.Postgres
{
    internal class PostgresViewStore : IViewStore
    {
        private const string LastDeletedViewId = "LastDeleted-b24ae98262724d27bd8e31c34ff11f1a";
        
        private readonly string _connectionString;
        private readonly string _schemaName;
        private readonly string _tableName;
        private readonly string _tablePath;

        public PostgresViewStore(
            string connectionString,
            string schemaName,
            string tableName)
        {
            _connectionString = connectionString;
            _schemaName = schemaName;
            _tableName = tableName;
            _tablePath = $"{schemaName}.{tableName}";
        }
        
        public GlobalVersion? ReadLastGlobalVersion()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            
            using var cmd = connection.ToReadLastGlobalVersionCommandOn(_tablePath);
            using var reader = cmd.ExecuteReader();

            GlobalVersion? globalVersion = reader.Read()
                ? GlobalVersion.Of(reader.GetInt64(0))
                : null;
            
            return globalVersion;
        }

        public async Task<GlobalVersion?> ReadLastGlobalVersionAsync()
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            
            await using var cmd = connection.ToReadLastGlobalVersionCommandOn(_tablePath);
            await using var reader = await cmd.ExecuteReaderAsync();

            GlobalVersion? globalVersion = await reader.ReadAsync()
                ? GlobalVersion.Of(reader.GetInt64(0))
                : null;
            
            return globalVersion;
        }

        public ViewEnvelope? Read(string viewId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var cmd = connection.ToReadViewIdCommandOn(viewId, _tablePath);
            using var reader = cmd.ExecuteReader();

            ViewEnvelope? globalVersion = reader.Read()
                ? new ViewEnvelopeInternal(
                    id: viewId,
                    view: reader.GetString(0),
                    viewType: reader.GetString(1),
                    shortViewType: reader.GetString(2),
                    metadata: reader.GetString(3),
                    globalVersion: reader.GetInt64(4),
                    tenantId: reader.GetString(5),
                    createdAt: reader.GetDateTime(6)).ToViewEnvelope()
                : null;
            
            return globalVersion;
        }

        public async Task<ViewEnvelope?> ReadAsync(string viewId)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            
            await using var cmd = connection.ToReadViewIdCommandOn(viewId, _tablePath);
            cmd.Parameters.AddWithValue("viewId", viewId);
            await using var reader = await cmd.ExecuteReaderAsync();

            ViewEnvelope? globalVersion = await reader.ReadAsync()
                ? new ViewEnvelopeInternal(
                    id: viewId,
                    view: reader.GetString(0),
                    viewType: reader.GetString(1),
                    shortViewType: reader.GetString(2),
                    metadata: reader.GetString(3),
                    globalVersion: reader.GetInt64(4),
                    tenantId: reader.GetString(5),
                    createdAt: reader.GetDateTime(6)).ToViewEnvelope()
                : null;
            
            return globalVersion;
        }

        public void Save(ViewEnvelope viewEnvelope) => Save(new[] { viewEnvelope });

        public Task SaveAsync(ViewEnvelope viewEnvelope) => SaveAsync(new[] { viewEnvelope });

        public void Save(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            SaveInternal(connection, transaction, viewEnvelopes);
            
            transaction.Commit();
        }
        
        private void SaveInternal(NpgsqlConnection connection, NpgsqlTransaction transaction, IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            using var batch = new NpgsqlBatch(connection, transaction);
            foreach (var viewEnvelope in viewEnvelopes)
            {
                batch.BatchCommands.Add(viewEnvelope.ToInternal().ToBatchCommandOn(_tablePath));
            }

            batch.ExecuteNonQuery();
        }

        public async Task SaveAsync(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            await SaveInternalAsync(connection, transaction, viewEnvelopes);
            
            await transaction.CommitAsync();
        }
        
        private Task SaveInternalAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            using var batch = new NpgsqlBatch(connection, transaction);
            foreach (var viewEnvelope in viewEnvelopes)
            {
                batch.BatchCommands.Add(viewEnvelope.ToInternal().ToBatchCommandOn(_tablePath));
            }

            return batch.ExecuteNonQueryAsync();
        }

        public void Delete(string viewId, GlobalVersion globalVersion) => Delete(new[] { viewId }, globalVersion);

        public Task DeleteAsync(string viewId, GlobalVersion globalVersion) => DeleteAsync(new[] { viewId }, globalVersion);

        public void Delete(IEnumerable<string> viewIds, GlobalVersion globalVersion)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            
            using var batch = new NpgsqlBatch(connection, transaction);
            foreach (var viewId in viewIds)
            {
                batch.BatchCommands.Add(viewId.ToBatchCommandOn(_tablePath));
            }

            batch.ExecuteNonQuery();
            
            SaveInternal(connection, transaction, new [] { ViewEnvelope.EmptyWith(LastDeletedViewId, globalVersion) });
            
            transaction.Commit();
        }

        public async Task DeleteAsync(IEnumerable<string> viewIds, GlobalVersion globalVersion)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            await using var transaction = await connection.BeginTransactionAsync();

            await using var batch = new NpgsqlBatch(connection, transaction);
            foreach (var viewId in viewIds)
            {
                batch.BatchCommands.Add(viewId.ToBatchCommandOn(_tablePath));
            }

            await batch.ExecuteNonQueryAsync();
            
            await SaveInternalAsync(connection, transaction, new [] { ViewEnvelope.EmptyWith(LastDeletedViewId, globalVersion) });
            
            await transaction.CommitAsync();
        }
    }
}

