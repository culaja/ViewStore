using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using ViewStore.Abstractions;

namespace ViewStore.Postgres
{
    internal class PostgresViewStore : IViewStore
    {
        private const string LastDeletedViewId = "LastDeleted-b24ae98262724d27bd8e31c34ff11f1a";
        
        private readonly string _connectionString;
        private readonly string _tablePath;

        public PostgresViewStore(
            string connectionString,
            string schemaName,
            string tableName)
        {
            _connectionString = connectionString;
            _tablePath = $"{schemaName}.{tableName}";
        }
        
        public GlobalVersion? ReadLastGlobalVersion()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            
            var sql = @$"
                select 
                    globalVersion 
                from {_tablePath}
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
                from {_tablePath}
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
                    (metadata)::text,
                    globalVersion
                from {_tablePath}
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
                    reader.GetInt64(3)).ToViewEnvelope()
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
                    (metadata)::text,
                    globalVersion
                from {_tablePath}
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
                    reader.GetInt64(3)).ToViewEnvelope()
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

            using var batch = new NpgsqlBatch(connection, transaction);
            foreach (var viewEnvelope in viewEnvelopes)
            {
                batch.BatchCommands.Add(viewEnvelope.ToInternal().ToBatchCommandOn(_tablePath));
            }

            batch.ExecuteNonQuery();
            
            transaction.Commit();
        }

        public async Task SaveAsync(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            
            await using var batch = new NpgsqlBatch(connection, transaction);
            foreach (var viewEnvelope in viewEnvelopes)
            {
                batch.BatchCommands.Add(viewEnvelope.ToInternal().ToBatchCommandOn(_tablePath));
            }

            await batch.ExecuteNonQueryAsync();
            
            await transaction.CommitAsync();
        }

        public void Delete(string viewId, GlobalVersion globalVersion)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            
            var sql = @$"
                DELETE FROM {_tablePath}
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
                DELETE FROM {_tablePath}
                WHERE id = @viewId";

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("viewId", viewId);
            
            await cmd.ExecuteNonQueryAsync();
            
            await CreateBatchCommandFrom(connection, transaction, ViewEnvelope.EmptyWith(LastDeletedViewId, globalVersion));
            
            await transaction.CommitAsync();
        }

        public void Delete(IEnumerable<string> viewIds, GlobalVersion globalVersion)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            
            var sql = @$"
                DELETE FROM {_tablePath}
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
                DELETE FROM {_tablePath}
                WHERE id = @viewId";

            foreach (var viewId in viewIds)
            {
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("viewId", viewId);
                await cmd.ExecuteNonQueryAsync();
            }
            
            await CreateBatchCommandFrom(connection, transaction, ViewEnvelope.EmptyWith(LastDeletedViewId, globalVersion));
            
            await transaction.CommitAsync();
        }
    }
}

