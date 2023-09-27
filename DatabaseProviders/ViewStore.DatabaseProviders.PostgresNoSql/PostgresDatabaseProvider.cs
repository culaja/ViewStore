using Npgsql;

namespace ViewStore.DatabaseProviders.PostgresNoSql;

internal sealed class PostgresDatabaseProvider : IDatabaseProvider
{
    private const string LastDeletedViewId = "LastGlobalPosition-b24ae98262724d27bd8e31c34ff11f1a";
    
    private readonly string _connectionString;
    private readonly string _schemaName;
    private readonly string _tableName;
    private readonly string _tablePath;
    private readonly AutoCreateOptions _autoCreateOptions;
    private bool _isCreated;

    public PostgresDatabaseProvider(
        string connectionString,
        string schemaName,
        string tableName,
        AutoCreateOptions autoCreateOptions)
    {
        _connectionString = connectionString;
        _schemaName = schemaName;
        _tableName = tableName;
        _autoCreateOptions = autoCreateOptions;
        _tablePath = $"{schemaName}.{tableName}";
    }
    
    public async Task<long?> ReadLastGlobalVersionAsync()
    {
        AutoCreateIfNeeded();

        await using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        await using var cmd = connection.ToReadLastGlobalVersionCommandOn(_tablePath);
        await using var reader = await cmd.ExecuteReaderAsync();

        long? globalVersion = reader.Read()
            ? reader.GetInt64(0)
            : null;
            
        return globalVersion;
    }

    public Task SaveLastGlobalVersionAsync(long globalVersion)
    {
        AutoCreateIfNeeded();
        
        return UpsertAsync(new[] { ViewRecord.EmptyWith(LastDeletedViewId, globalVersion) });
    }

    public async Task<ViewRecord?> ReadAsync(string viewId)
    {
        AutoCreateIfNeeded();
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
            
        await using var cmd = connection.ToReadViewIdCommandOn(viewId, _tablePath);
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
        AutoCreateIfNeeded();
        
        await using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        
        await using var transaction = await connection.BeginTransactionAsync();
        
        await using var batch = new NpgsqlBatch(connection, transaction);
        foreach (var viewRecord in viewRecords)
        {
            batch.BatchCommands.Add(viewRecord.ToInternal().ToBatchCommandOn(_tablePath));
        }

        var count = await batch.ExecuteNonQueryAsync();
        await transaction.CommitAsync();
        
        return count;
    }

    public async Task<long> DeleteAsync(IEnumerable<string> viewIds)
    {
        AutoCreateIfNeeded();
        
        await using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();

        await using var batch = new NpgsqlBatch(connection, transaction);
        foreach (var viewId in viewIds)
        {
            batch.BatchCommands.Add(viewId.ToBatchCommandOn(_tablePath));
        }

        var count = await batch.ExecuteNonQueryAsync();
        await transaction.CommitAsync();

        return count;
    }
    
    private void AutoCreateIfNeeded()
    {
        if (!_isCreated)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            
            if (_autoCreateOptions.ShouldCreateSchema) CreateSchema(connection, _schemaName);
            if (_autoCreateOptions.ShouldCreateTable) CreateTable(connection, _schemaName, _tableName, _autoCreateOptions.ShouldCreateUnLoggedTable);
            _autoCreateOptions.PostCreationScriptProvider?.Invoke(connection, _schemaName, _tableName);
            
            connection.Close();
            _isCreated = true;
        }
    }
    
    private void CreateSchema(NpgsqlConnection connection, string schemaName)
    {
        var sql = $"CREATE SCHEMA IF NOT EXISTS {schemaName}";
        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.ExecuteNonQuery();
    }

    private void CreateTable(NpgsqlConnection connection, string schemaName, string tableName, bool shouldCreateUnLoggedTable)
    {
        var unloggedSqlPart = shouldCreateUnLoggedTable ? " UNLOGGED " : "";
        var sql = $@"
                CREATE {unloggedSqlPart} TABLE IF NOT EXISTS {schemaName}.{tableName}(
                    id varchar(256) NOT NULL,
                    view jsonb NOT NULL,
                    viewType varchar(1024) NOT NULL,
                    shortViewType varchar(128) NOT NULL,
                    globalVersion bigint NOT NULL,
                    lastChangeTimeStamp timestamp NOT NULL,
                    CONSTRAINT {schemaName}_{tableName}_pkey PRIMARY KEY (id));";
            
        using var cmd = new NpgsqlCommand(sql, connection);
            
        cmd.ExecuteNonQuery();
    }
}