using System;
using System.Diagnostics;
using MongoDB.Driver;
using ViewStore.Abstractions;
using ViewStore.MongoDb;
using ViewStore.WriteThroughCache;

namespace ViewStore.PerformanceTestsnceTests
{
    class Program
    {
        static void Main()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017/TestDb");
            var mongoDatabase = mongoClient.GetDatabase("TestDb");
            var mongoViewStore = new MongoDbViewStore(mongoDatabase, nameof(UsersLoggedInInHour));
            
            var viewStoreCache = ViewStoreCacheFactory.New()
                .WithCacheDrainPeriod(TimeSpan.FromMilliseconds(1000))
                .WithCacheDrainBatchSize(500)
                .For(mongoViewStore)
                .UseCallbackWhenDrainFinished(Console.WriteLine)
                .Build();

            ExecuteTest(viewStoreCache);
        }

        private static void ExecuteTest(ViewStoreCache viewStoreCache)
        {
            var sw = new Stopwatch();
            sw.Start();

            using (viewStoreCache)
            {
                var startGlobalVersion = viewStoreCache.ReadLastKnownPosition() ?? 1;
                Console.WriteLine($"Start global version: {startGlobalVersion}");
                var documentIdCounter = startGlobalVersion % 100;
            
                for (var nextGlobalVersion = startGlobalVersion; nextGlobalVersion <= 2190000; nextGlobalVersion++)
                {
                    var documentId = documentIdCounter.ToString();
                    if (nextGlobalVersion % 100 == 0)
                    {
                        documentIdCounter++;
                    }
                
                    var optionalView = viewStoreCache.Read<UsersLoggedInInHour>(documentId);
                    if (optionalView == null)
                    {
                        optionalView = new UsersLoggedInInHour(documentId, nextGlobalVersion);
                    }
                
                    viewStoreCache.Save(optionalView.Increment(nextGlobalVersion));
                }
            }
            
            sw.Stop();
            Console.WriteLine($"[Elapsed time] {sw.ElapsedMilliseconds / 1000m}");
        }
    }
}