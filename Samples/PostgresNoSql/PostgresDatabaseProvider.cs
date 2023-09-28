using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using ViewStore.Cache;

namespace PostgresNoSql;

internal sealed class PostgresDatabaseProvider : IDatabaseProvider
{
    private const string LastGlobalPosition = "LastGlobalPosition-b24ae98262724d27bd8e31c34ff11f1a";
    
    private readonly string _connectionString;
    private readonly string _schemaName;
    private readonly string _tableName;
    private readonly string _tablePath;

    public PostgresDatabaseProvider(
        string connectionString,
        string schemaName,
        string tableName)
    {
        _connectionString = connectionString;
        _schemaName = schemaName;
        _tableName = tableName;
        _tablePath = $"{schemaName}.{tableName}";
    }

    public async Task PrepareSchema()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        {
            await using var cmd = new NpgsqlCommand($"CREATE SCHEMA IF NOT EXISTS {_schemaName}", connection);
            await cmd.ExecuteNonQueryAsync();    
        }

        {
            await using var cmd = new NpgsqlCommand($@"
            CREATE TABLE IF NOT EXISTS {_schemaName}.{_tableName}(
                id varchar(256) NOT NULL,
                view jsonb NOT NULL,
                viewType varchar(1024) NOT NULL,
                shortViewType varchar(128) NOT NULL,
                globalVersion bigint NOT NULL,
                lastChangeTimeStamp timestamp NOT NULL,
                CONSTRAINT {_schemaName}_{_tableName}_pkey PRIMARY KEY (id));", connection);
            
            await cmd.ExecuteNonQueryAsync();    
        }
    }

    public async Task<long?> ReadLastGlobalVersionAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
            
        await using var cmd = new NpgsqlCommand(@$"
                select 
                    globalVersion 
                from {_tablePath}
                    where globalVersion = '{LastGlobalPosition}'
                limit 1", connection);
        await using var reader = await cmd.ExecuteReaderAsync();

        long? globalVersion = await reader.ReadAsync()
            ? reader.GetInt64(0)
            : null;
            
        return globalVersion;
    }

    public Task SaveLastGlobalVersionAsync(long globalVersion) => 
        UpsertAsync(new[] { ViewRecord.EmptyWith(LastGlobalPosition, globalVersion) });

    public async Task<ViewRecord?> ReadAsync(string viewId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var cmd = new NpgsqlCommand(@$"
                select
                    (view)::text,
                    viewType,
                    shortViewType,
                    globalVersion
                from {_tablePath}
                    where id = @viewId", connection);
        cmd.Parameters.AddWithValue("viewId", viewId);
        
        await using var reader = await cmd.ExecuteReaderAsync();

        ViewRecord? viewRecord = await reader.ReadAsync()
            ? new ViewRecordInternal(
                id: viewId,
                view: reader.GetString(0),
                viewType: reader.GetString(1),
                shortViewType: reader.GetString(2),
                globalVersion: reader.GetInt64(4)).ToViewRecord()
            : null;
            
        return viewRecord;
    }

    public async Task<long> UpsertAsync(IEnumerable<ViewRecord> viewRecords)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        
        await using var transaction = await connection.BeginTransactionAsync();
        
        await using var batch = new NpgsqlBatch(connection, transaction);
        foreach (var viewRecord in viewRecords)
        {
            var batchCommand = new NpgsqlBatchCommand(@$"
                INSERT INTO {_tableName}
                (
                    id,
                    view,
                    viewType,
                    shortViewType,
                    globalVersion,
                    lastChangeTimeStamp
                ) 
                VALUES
                (
                    @viewId,
                    @view,
                    @viewType,
                    @shortViewType,
                    @globalVersion,
                    @lastChangeTimeStamp
                ) 
                ON CONFLICT (id) DO UPDATE
                SET 
                    view = @view,
                    viewType = @viewType,
                    shortViewType = @shortViewType,
                    globalVersion = @globalVersion,
                    lastChangeTimeStamp = @lastChangeTimeStamp;");
            var viewRecordInternal = new ViewRecordInternal(viewRecord);
            batchCommand.Parameters.AddWithValue("viewId", viewRecordInternal.Id);
            batchCommand.Parameters.AddWithValue("view", NpgsqlDbType.Jsonb, viewRecordInternal.View);
            batchCommand.Parameters.AddWithValue("viewType", viewRecordInternal.ViewType);
            batchCommand.Parameters.AddWithValue("shortViewType", viewRecordInternal.ShortViewType);
            batchCommand.Parameters.AddWithValue("globalVersion", viewRecordInternal.GlobalVersion);
            batchCommand.Parameters.AddWithValue("lastChangeTimeStamp", DateTime.UtcNow);
            batch.BatchCommands.Add(batchCommand);
        }

        var count = await batch.ExecuteNonQueryAsync();
        await transaction.CommitAsync();
        
        return count;
    }

    public async Task<long> DeleteAsync(IEnumerable<string> viewIds)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();

        await using var batch = new NpgsqlBatch(connection, transaction);
        foreach (var viewId in viewIds)
        {
            var batchCommand = new NpgsqlBatchCommand($"DELETE FROM {_tableName} WHERE id = @viewId");
            batchCommand.Parameters.AddWithValue("viewId", viewId);
            batch.BatchCommands.Add(batchCommand);
        }

        var count = await batch.ExecuteNonQueryAsync();
        await transaction.CommitAsync();

        return count;
    }
    
    private sealed class ViewRecordInternal
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new() { 
            TypeNameHandling = TypeNameHandling.All
        };
    
        public string Id { get; }
        public string View { get; }
        public string ViewType { get; }
        public string ShortViewType { get; }
        public long GlobalVersion { get; }

        public ViewRecordInternal(
            string id,
            string view,
            string viewType,
            string shortViewType,
            long globalVersion)
        {
            Id = id;
            View = view;
            ViewType = viewType;
            ShortViewType = shortViewType;
            GlobalVersion = globalVersion;
        }

        public ViewRecordInternal(ViewRecord viewRecord) : this(
            viewRecord.Id,
            JsonConvert.SerializeObject(viewRecord.View, JsonSerializerSettings),
            viewRecord.View.GetType().AssemblyQualifiedName!,
            viewRecord.View.GetType().Name,
            viewRecord.GlobalVersion)
        {
        }

        public ViewRecord ToViewRecord() => new(
            Id,
            (JsonConvert.DeserializeObject(View, Type.GetType(ViewType)!) as IView)!, 
            GlobalVersion);
    }
}