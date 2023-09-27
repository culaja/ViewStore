namespace ViewStore.DatabaseProviders.Postgres;

public sealed class PostgresDatabaseProviderBuilder
{
    private string? _connectionString;
    private string _schemaName = "public";
    private string? _tableName;
    private AutoCreateOptions? _autoCreateOptions;
        
    public static PostgresDatabaseProviderBuilder New() => new();
        
    public PostgresDatabaseProviderBuilder WithConnectionString(string connectionString)
    {
        _connectionString = connectionString;
        return this;
    }
        
    public PostgresDatabaseProviderBuilder WithSchemaName(string schemaName)
    {
        _schemaName = schemaName;
        return this;
    }
        
    public PostgresDatabaseProviderBuilder WithTableName(string tableName)
    {
        _tableName = tableName;
        return this;
    }

    public PostgresDatabaseProviderBuilder WithTablePath(string tablePath)
    {
        var splitTablePath = tablePath.Split(".");
        if (splitTablePath.Length != 2) throw new ArgumentException("Table path should be in <schemaName>.<tableName> format", nameof(tablePath));
        _schemaName = splitTablePath[0];
        _tableName = splitTablePath[1];
        return this;
    }
        
    public PostgresDatabaseProviderBuilder WithAutoCreateOptions(AutoCreateOptions? autoCreateOptions = null)
    {
        _autoCreateOptions = autoCreateOptions;
        return this;
    }
        
    public IDatabaseProvider Build()
    {
        if (_connectionString == null) throw new ArgumentNullException(nameof(_connectionString), "Connection string is not provided");
        if (_tableName == null) throw new ArgumentNullException(nameof(_tableName), "Table name is not provided");
            
        var provider = new PostgresDatabaseProvider(
            _connectionString,
            _schemaName,
            _tableName,
            _autoCreateOptions ?? new AutoCreateOptions(true, true, null));

        return provider;
    }
}