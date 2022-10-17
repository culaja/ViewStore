namespace ViewStore.Postgres
{
    public sealed class AutoCreateOptions
    {
        public bool ShouldCreateSchema { get; }
        public bool ShouldCreateTable { get; }
        public PostCreationScriptProvider? PostCreationScriptProvider { get; }

        public AutoCreateOptions(
            bool shouldCreateSchema,
            bool shouldCreateTable,
            PostCreationScriptProvider? postCreationScriptProvider)
        {
            ShouldCreateSchema = shouldCreateSchema;
            ShouldCreateTable = shouldCreateTable;
            PostCreationScriptProvider = postCreationScriptProvider;
        }
        
    }
}