namespace ViewStore.Postgres
{
    public sealed class AutoCreateOptions
    {
        public bool ShouldCreateSchema { get; }
        public bool ShouldCreateTable { get; }
        /// <summary>
        /// Details about UNLOGGED tables: https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-UNLOGGED
        /// </summary>
        public bool ShouldCreateUnLoggedTable { get; }
        public PostCreationScriptProvider? PostCreationScriptProvider { get; }

        public AutoCreateOptions(
            bool shouldCreateSchema,
            bool shouldCreateTable,
            PostCreationScriptProvider? postCreationScriptProvider,
            bool shouldCreateUnLoggedTable = false)
        {
            ShouldCreateSchema = shouldCreateSchema;
            ShouldCreateTable = shouldCreateTable;
            ShouldCreateUnLoggedTable = shouldCreateUnLoggedTable;
            PostCreationScriptProvider = postCreationScriptProvider;
        }
        
    }
}