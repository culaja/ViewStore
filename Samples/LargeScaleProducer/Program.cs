using Newtonsoft.Json;
using ViewStore.Abstractions;
using ViewStore.Cache;
using ViewStore.Postgres;

var viewStore = ViewStoreCacheBuilder.New()
    .For(PostgresViewStoreBuilder.New()
        .WithConnectionString(connectionString: "host=localhost;port=5432;database=ViewStore;password=o;username=postgres")
        .WithTablePath(tablePath: "test.large_scale")
        .ShouldAutoCreate(
            shouldAutoCreate: true,
            autoCreateOptions: new AutoCreateOptions(
                shouldCreateSchema: true,
                shouldCreateTable: true,
                postCreationScriptProvider: null,
                shouldCreateUnLoggedTable: true))
        .Build())
    .WithCacheDrainBatchSize(500)
    .WithThrottleAfterCacheCount(1000)
    .WithCacheDrainPeriod(TimeSpan.FromSeconds(1))
    .UseCallbackWhenDrainFinished(drainStatistics => Console.WriteLine($"[CACHE] Cache drained into DB ({JsonConvert.SerializeObject(drainStatistics)})"))
    .UseCallbackOnThrottling(throttlingStatistics => Console.WriteLine($"[CACHE] Throttling since cache draining is too slow for incoming traffic ({JsonConvert.SerializeObject(throttlingStatistics)})"))
    .UseCallbackOnDrainAttemptFailed(ex => Console.WriteLine($"[CACHE] Exception received while draining cache: {ex.Message}"))
    .Build();

for (var i = 1L;; ++i)
{
    var eventEnvelope = new ViewEnvelope($"{nameof(SampleView)}-{i}", new SampleView(i), GlobalVersion.Of(i), MetaData.New());
    viewStore.Save(eventEnvelope);
}

internal sealed class SampleView : IView
{
    public long Index { get; }

    public SampleView(long index)
    {
        Index = index;
    }
}