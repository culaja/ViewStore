using Npgsql;

namespace ViewStore.DatabaseProviders.Postgres;

public delegate void PostCreationScriptProvider(NpgsqlConnection connection, string schemaName, string tableName);

public sealed class AutoCreateOptions
{
    public bool ShouldCreateSchema { get; }
    public bool ShouldCreateTable { get; }
    public bool ShouldCreateUnLoggedTable { get; }
    public PostCreationScriptProvider? PostCreationScriptProvider { get; }

    public AutoCreateOptions(
        bool shouldCreateSchema,
        bool shouldCreateTable,
        PostCreationScriptProvider? postCreationScriptProvider = null,
        bool shouldCreateUnLoggedTable = false)
    {
        ShouldCreateSchema = shouldCreateSchema;
        ShouldCreateTable = shouldCreateTable;
        ShouldCreateUnLoggedTable = shouldCreateUnLoggedTable;
        PostCreationScriptProvider = postCreationScriptProvider;
    }
}