using PostgresNoSql;
using ViewStore.Cache;
using ViewStore.Cache.Cache;

using var cache = ViewStoreCacheBuilder.New()
    .WithDatabaseProvider(new PostgresDatabaseProvider(
        connectionString: "host=localhost;port=5432;database=viewstore;password=o;username=postgres",
        schemaName: "public",
        tableName: "test_view"))
    .UseCallbackWhenDrainFinished(ds => Console.WriteLine(ds.ToString()))
    .UseCallbackOnDrainAttemptFailed(Console.WriteLine)
    .Build();

cache.Save(new User(Id: "1", Name: "Stanko", Age: 35).ToRecord(v => v.Id));
cache.Save(new User(Id: "2", Name: "Marko", Age: 35).ToRecord(v => v.Id));
cache.Save(new User(Id: "3", Name: "Milenko", Age: 34).ToRecord(v => v.Id));
cache.Delete("2");

namespace PostgresNoSql
{
    internal sealed record User(
        string Id,
        string Name,
        int Age) : IView;
}