using System;
using System.Diagnostics;
using MongoDB.Driver;
using ViewStore.Abstractions;
using ViewStore.Cache;
using ViewStore.MongoDb;

namespace ViewStore.PerformanceTestsnceTests
{
    class Program
    {
        static void Main()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017/TestDb");
            var mongoDatabase = mongoClient.GetDatabase("TestDb");
            var mongoViewStore = new MongoDbViewStore(mongoDatabase, nameof(UsersLoggedInInHour));


            var (viewStore, disposable) = ViewStoreCacheFactory.New()
                .WithCacheItemExpirationPeriod(TimeSpan.FromHours(1))
                .WithCacheDrainPeriod(TimeSpan.FromMilliseconds(1000))
                .WithCacheDrainBatchSize(500)
                .For(mongoViewStore)
                .UseCallbackWhenDrainFinished(drainedViews => Console.WriteLine(drainedViews.Count))
                .Build();

            using (disposable)
            {
                ExecuteTest(viewStore);
            }
        }

        private static void ExecuteTest(IViewStore viewStore)
        {
            var sw = new Stopwatch();
            sw.Start();
            
            var startGlobalVersion = viewStore.ReadLastKnownPosition() ?? 1;
            Console.WriteLine($"Start global version: {startGlobalVersion}");
            var documentIdCounter = startGlobalVersion % 100;
            
            for (var nextGlobalVersion = startGlobalVersion; nextGlobalVersion <= 2190000; nextGlobalVersion++)
            {
                var documentId = documentIdCounter.ToString();
                if (nextGlobalVersion % 100 == 0)
                {
                    documentIdCounter++;
                }
                
                var optionalView = viewStore.Read<UsersLoggedInInHour>(documentId);
                if (optionalView == null)
                {
                    optionalView = new UsersLoggedInInHour(documentId, nextGlobalVersion);
                }
                
                viewStore.Save(optionalView.Increment(nextGlobalVersion));
            }
            
            sw.Stop();
            
            Console.WriteLine($"[{viewStore.GetType().Name}] {sw.ElapsedMilliseconds / 1000m}");
        }
    }
}