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

        public PostgresViewStore(
            string connectionString,
            string schemaName,
            string tableName)
        {
            _connectionString = connectionString;
            _schemaName = schemaName;
            _tableName = tableName;
        }
        
        public GlobalVersion? ReadLastGlobalVersion()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            
            var sql = @$"
                select 
                    globalVersion 
                from {_schemaName}.{_tableName}
                    order by globalVersion desc
                limit 1";
            
            using var cmd = new NpgsqlCommand(sql, connection);
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
            
            var sql = @$"
                select 
                    globalVersion 
                from {_schemaName}.{_tableName}
                    order by globalVersion desc
                limit 1";

            await using var cmd = new NpgsqlCommand(sql, connection);
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
            
            var sql = @$"
                select
                    (view)::text,
                    viewType,
                    shortViewType,
                    (metadata)::text,
                    globalVersion,
                    tenantId,
                    createdAt
                from {_schemaName}.{_tableName}
                    where id = @viewId";
            
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("viewId", viewId);
            using var reader = cmd.ExecuteReader();

            ViewEnvelope? globalVersion = reader.Read()
                ? new ViewEnvelopeInternal(
                    viewId,
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetInt64(4),
                    reader.GetString(5),
                    reader.GetDateTime(6)).ToViewEnvelope()
                : null;
            
            return globalVersion;
        }

        public async Task<ViewEnvelope?> ReadAsync(string viewId)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var sql = @$"
                select
                    (view)::text,
                    viewType,
                    shortViewType,
                    (metadata)::text,
                    globalVersion,
                    tenantId,
                    createdAt
                from {_schemaName}.{_tableName}
                    where id = @viewId";

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("viewId", viewId);
            await using var reader = await cmd.ExecuteReaderAsync();

            ViewEnvelope? globalVersion = await reader.ReadAsync()
                ? new ViewEnvelopeInternal(
                    viewId,
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetInt64(4),
                    reader.GetString(5),
                    reader.GetDateTime(6)).ToViewEnvelope()
                : null;
            
            return globalVersion;
        }

        public void Save(ViewEnvelope viewEnvelope)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            
            SaveInternal(connection, null, viewEnvelope);
        }

        private void SaveInternal(
            NpgsqlConnection connection, 
            NpgsqlTransaction? transaction,
            ViewEnvelope viewEnvelope)
        {
            var sql = @$"
                INSERT INTO {_schemaName}.{_tableName}
                (
                    id,
                    view,
                    viewType,
                    shortViewType,
                    metadata,
                    globalVersion,
                    lastChangeTimeStamp,
                    tenantId,
                    createdAt
                ) 
                VALUES
                (
                    @viewId,
                    @view,
                    @viewType,
                    @shortViewType,
                    @metadata,
                    @globalVersion,
                    @lastChangeTimeStamp,
                    @tenantId,
                    @createdAt
                )
                ON CONFLICT (id,tenantId,createdAt) DO UPDATE
                SET 
                    view = @view,
                    viewType = @viewType,
                    shortViewType = @shortViewType,
                    metadata = @metadata,
                    globalVersion = @globalVersion,
                    lastChangeTimeStamp = @lastChangeTimeStamp,
                    tenantId = @tenantId,
                    createdAt = @createdAt";
            
            using var cmd = new NpgsqlCommand(sql, connection, transaction);
            var viewEnvelopeInternal = new ViewEnvelopeInternal(viewEnvelope);
            cmd.Parameters.AddWithValue("viewId", viewEnvelopeInternal.Id);
            cmd.Parameters.AddWithValue("view", NpgsqlDbType.Jsonb, viewEnvelopeInternal.View);
            cmd.Parameters.AddWithValue("viewType", viewEnvelopeInternal.ViewType);
            cmd.Parameters.AddWithValue("shortViewType", viewEnvelopeInternal.ShortViewType);
            cmd.Parameters.AddWithValue("metadata", NpgsqlDbType.Jsonb, viewEnvelopeInternal.Metadata);
            cmd.Parameters.AddWithValue("globalVersion", viewEnvelopeInternal.GlobalVersion);
            cmd.Parameters.AddWithValue("lastChangeTimeStamp", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("tenantId", viewEnvelopeInternal.TenantId);
            cmd.Parameters.AddWithValue("createdAt", viewEnvelopeInternal.CreatedAt);
            
            cmd.ExecuteNonQuery();
        }

        public async Task SaveAsync(ViewEnvelope viewEnvelope)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await SaveInternalAsync(connection, null, viewEnvelope);
        }

        private async Task SaveInternalAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction? transaction,
            ViewEnvelope viewEnvelope)
        {
            var sql = @$"
                INSERT INTO {_schemaName}.{_tableName}
                (
                    id,
                    view,
                    viewType,
                    shortViewType,
                    metadata,
                    globalVersion,
                    lastChangeTimeStamp,
                    tenantId,
                    createdAt
                ) 
                VALUES
                (
                    @viewId,
                    @view,
                    @viewType,
                    @shortViewType,
                    @metadata,
                    @globalVersion,
                    @lastChangeTimeStamp,
                    @tenantId,
                    @createdAt
                ) 
                ON CONFLICT (id,tenantId,createdAt) DO UPDATE
                SET 
                    view = @view,
                    viewType = @viewType,
                    shortViewType = @shortViewType,
                    metadata = @metadata,
                    globalVersion = @globalVersion,
                    lastChangeTimeStamp = @lastChangeTimeStamp,
                    tenantId = @tenantId,
                    createdAt = @createdAt";

            await using var cmd = new NpgsqlCommand(sql, connection, transaction);
            var viewEnvelopeInternal = new ViewEnvelopeInternal(viewEnvelope);
            cmd.Parameters.AddWithValue("viewId", viewEnvelopeInternal.Id);
            cmd.Parameters.AddWithValue("view", NpgsqlDbType.Jsonb, viewEnvelopeInternal.View);
            cmd.Parameters.AddWithValue("viewType", viewEnvelopeInternal.ViewType);
            cmd.Parameters.AddWithValue("shortViewType", viewEnvelopeInternal.ShortViewType);
            cmd.Parameters.AddWithValue("metadata", NpgsqlDbType.Jsonb, viewEnvelopeInternal.Metadata);
            cmd.Parameters.AddWithValue("globalVersion", viewEnvelopeInternal.GlobalVersion);
            cmd.Parameters.AddWithValue("lastChangeTimeStamp", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("tenantId", viewEnvelopeInternal.TenantId);
            cmd.Parameters.AddWithValue("createdAt", viewEnvelopeInternal.CreatedAt);
            
            await cmd.ExecuteNonQueryAsync();
        }
        

        public void Save(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            
            foreach (var viewEnvelope in viewEnvelopes)
            {
                SaveInternal(connection, transaction, viewEnvelope);
            }
            
            transaction.Commit();
        }

        public async Task SaveAsync(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            
            foreach (var viewEnvelope in viewEnvelopes)
            {
                await SaveInternalAsync(connection, transaction, viewEnvelope);
            }
            
            await transaction.CommitAsync();
        }

        public void Delete(string viewId, GlobalVersion globalVersion)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            
            var sql = @$"
                DELETE FROM {_schemaName}.{_tableName}
                WHERE id = @viewId";
            
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("viewId", viewId);
            
            cmd.ExecuteNonQuery();
            
            SaveInternal(connection, transaction, ViewEnvelope.EmptyWith(LastDeletedViewId, globalVersion));
            
            transaction.Commit();
        }

        public async Task DeleteAsync(string viewId, GlobalVersion globalVersion)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            
            var sql = @$"
                DELETE FROM {_schemaName}.{_tableName}
                WHERE id = @viewId";

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("viewId", viewId);
            
            await cmd.ExecuteNonQueryAsync();
            
            await SaveInternalAsync(connection, transaction, ViewEnvelope.EmptyWith(LastDeletedViewId, globalVersion));
            
            await transaction.CommitAsync();
        }

        public void Delete(IEnumerable<string> viewIds, GlobalVersion globalVersion)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            
            var sql = @$"
                DELETE FROM {_schemaName}.{_tableName}
                WHERE id = @viewId";

            foreach (var viewId in viewIds)
            {
                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("viewId", viewId);
                cmd.ExecuteNonQuery();
            }
            
            SaveInternal(connection, transaction, ViewEnvelope.EmptyWith(LastDeletedViewId, globalVersion));
            
            transaction.Commit();
        }

        public async Task DeleteAsync(IEnumerable<string> viewIds, GlobalVersion globalVersion)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            
            var sql = @$"
                DELETE FROM {_schemaName}.{_tableName}
                WHERE id = @viewId";

            foreach (var viewId in viewIds)
            {
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("viewId", viewId);
                await cmd.ExecuteNonQueryAsync();
            }
            
            await SaveInternalAsync(connection, transaction, ViewEnvelope.EmptyWith(LastDeletedViewId, globalVersion));
            
            await transaction.CommitAsync();
        }
    }
}

