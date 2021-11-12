using System;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using EventStore.Client;
using MongoDB.Bson.Serialization;
using Spectre.Console;
using ViewStore.Abstractions;
using ViewStore.Cache;
using ViewStore.MongoDb;

namespace QueueingSystemEventProcessor
{
    class Program
    {
        private static readonly EventStoreClient EventStoreClient;
        private static readonly IViewStore ViewStore;
        
        static Program()
        {
            var settings = EventStoreClientSettings.Create("esdb://localhost:2111,localhost:2112,localhost:2113?tls=true&tlsVerifyCert=false");
            settings.DefaultCredentials = new UserCredentials("admin", "changeit");
            EventStoreClient = new EventStoreClient(settings);

            var mongoDbStore = MongoDbViewStoreBuilder.New()
                .UseViewRegistrator(() => BsonClassMap.RegisterClassMap<TotalStatisticsView>())
                .WithCollectionName($"{nameof(TotalStatisticsView)}_1")
                .WithConnectionDetails("mongodb://kolotree:kolotree4532@localhost:27018", "MyDb")
                .Build();

            ViewStore = ViewStoreCacheBuilder.New()
                .For(mongoDbStore)
                .WithCacheDrainPeriod(TimeSpan.FromSeconds(5))
                .UseCallbackWhenDrainFinished(_ => { })
                .UseCallbackOnDrainAttemptFailed(_ => { })
                .WithCacheDrainBatchSize(1000)
                .Build();
        }
        
        static void Main(string[] args)
        {
            AnsiConsole.Progress()
                .Columns(new ProgressColumn[] 
                {
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new TransferSpeedColumn(),
                    new SpinnerColumn(),
                    new ElapsedTimeColumn()
                })
                .Start(ctx => 
                {
                    var progressTask = ctx.AddTask("[green] Processed events [/]", new ProgressTaskSettings {MaxValue = 10000000});

                    var lastGlobalVersion = ViewStore.ReadLastGlobalVersion();
                    var startPosition = lastGlobalVersion.HasValue
                        ? new Position((ulong)lastGlobalVersion.Value.Value, (ulong)lastGlobalVersion.Value.Value)
                        : Position.Start;

                    EventStoreClient.SubscribeToAllAsync(
                        startPosition,
                        (_, resolvedEvent, _) =>
                        {
                            progressTask.Increment(1);
                            //ProcessEvent(resolvedEvent);
                            return Task.CompletedTask;
                        },
                        false,
                        (_, _, exception) => throw exception,
                        new SubscriptionFilterOptions(EventTypeFilter.RegularExpression(
                            $"{nameof(CustomerEnqueued)}|{nameof(CustomerDequeued)}|{nameof(CustomerAssigned)}|{nameof(CustomerServed)}")));

                    while (!ctx.IsFinished)
                    {
                        Thread.Sleep(1000);
                    }
                });
        }

        private static void ProcessEvent(ResolvedEvent resolvedEvent)
        {
            var e = resolvedEvent.ToEvent();
            ViewStore.SaveAsync(
                (ViewStore.Read(e.ViewId) ?? ViewEnvelope.NewOf(e.ViewId, TotalStatisticsView.New()))
                    .ImmutableTransform<TotalStatisticsView>(resolvedEvent.ToGlobalVersion(), v => v.Apply(e)));
        }
    }
}