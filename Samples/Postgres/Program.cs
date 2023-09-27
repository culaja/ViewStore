// See https://aka.ms/new-console-template for more information

using ViewStore;
using ViewStore.Cache;
using ViewStore.DatabaseProviders.Postgres;

var databaseProvider = PostgresDatabaseProviderBuilder.New()
    .WithConnectionString("host=localhost;port=5432;database=viewstore;password=o;username=postgres")
    .WithTablePath("public.test_view")
    .WithAutoCreateOptions(new AutoCreateOptions(shouldCreateSchema: false, shouldCreateTable: true))
    .Build();

using var cache = ViewStoreCacheBuilder.New()
    .WithDatabaseProvider(databaseProvider)
    .UseCallbackWhenDrainFinished(ds => Console.WriteLine(ds.ToString()))
    .Build();

cache.Save(new User(Id: "1", Name: "Stanko", Age: 35).ToRecord(v => v.Id));
cache.Save(new User(Id: "2", Name: "Marko", Age: 35).ToRecord(v => v.Id));
cache.Save(new User(Id: "3", Name: "Milenko", Age: 34).ToRecord(v => v.Id));

internal sealed record User(
    string Id,
    string Name,
    int Age) : IView;