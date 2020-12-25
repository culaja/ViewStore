using System;
using System.Diagnostics;
using System.Linq;
using Abstractions;
using Cache;
using MongoDB.Driver;
using Stores.MongoDb;

namespace PerformanceTests
{
    class Program
    {
        static void Main()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017/TestDb");
            var mongoDatabase = mongoClient.GetDatabase("TestDb");
            var mongoViewStore = new MongoDbViewStore<UsersLoggedInInHour>(mongoDatabase, nameof(UsersLoggedInInHour));
            var mongoViewStorePositionTracker = new MongoViewStorePositionTracker(mongoDatabase, nameof(UsersLoggedInInHour));


            var (viewStore, disposable) = ViewStoreCacheFactory<UsersLoggedInInHour>.New()
                .WithCacheItemExpirationPeriod(TimeSpan.FromHours(1))
                .WithCacheDrainPeriod(TimeSpan.FromMilliseconds(1000))
                .WithCacheDrainBatchSize(500)
                .For(mongoViewStore)
                .UseCallbackWhenDrainFinished(drainedViews => 
                    mongoViewStorePositionTracker.StoreLastKnownPosition(
                        drainedViews.OrderByDescending(t => t.GlobalVersion).First().GlobalVersion))
                .Build();

            using (disposable)
            {
                ExecuteTest(viewStore, mongoViewStorePositionTracker.ReadLastKnownPosition());
            }
        }

        private static void ExecuteTest(IViewStore<UsersLoggedInInHour> viewStore, long? lastKnownViewPosition)
        {
            var sw = new Stopwatch();
            sw.Start();
            
            var startGlobalVersion = lastKnownViewPosition ?? 1;
            Console.WriteLine($"Start global version: {startGlobalVersion}");
            var documentIdCounter = startGlobalVersion % 100;
            
            for (var nextGlobalVersion = startGlobalVersion; nextGlobalVersion <= 2190000; nextGlobalVersion++)
            {
                var documentId = documentIdCounter.ToString();
                if (nextGlobalVersion % 100 == 0)
                {
                    documentIdCounter++;
                }
                
                var optionalView = viewStore.Read(documentId);
                if (optionalView == null)
                {
                    optionalView = new UsersLoggedInInHour(documentId.ToString(), nextGlobalVersion);
                }
                
                viewStore.Save(optionalView.Increment(nextGlobalVersion));
            }
            
            sw.Stop();
            
            Console.WriteLine($"[{viewStore.GetType().Name}] {sw.ElapsedMilliseconds / 1000m}");
        }
    }
}