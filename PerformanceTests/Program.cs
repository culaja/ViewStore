using System;
using System.Diagnostics;
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
            //ExecuteTest(ClotMongoViewStore());
            ExecuteTest(CachedMongoViewStore());
        }

        private static void ExecuteTest((IViewStore<UsersLoggedInInHour> viewStore, IDisposable disposable) t)
        {
            var sw = new Stopwatch();
            sw.Start();
            using (t.disposable)
            {
                var startGlobalVersion = t.viewStore.ReadGlobalVersion() ?? 1;
                Console.WriteLine($"Start global version: {startGlobalVersion}");
                var documentIdCounter = startGlobalVersion % 100;
                
                for (var nextGlobalVersion = startGlobalVersion; nextGlobalVersion <= 2190000; nextGlobalVersion++)
                {
                    var documentId = documentIdCounter.ToString();
                    if (nextGlobalVersion % 100 == 0)
                    {
                        documentIdCounter++;
                    }
                    
                    var optionalView = t.viewStore.Read(documentId);
                    if (optionalView == null)
                    {
                        optionalView = new UsersLoggedInInHour(documentId.ToString(), nextGlobalVersion);
                    }
                    
                    t.viewStore.Save(optionalView.Increment(nextGlobalVersion));
                }
            }
            sw.Stop();
            
            Console.WriteLine($"[{t.viewStore.GetType().Name}] {sw.ElapsedMilliseconds / 1000m}");
        }

        private class NoDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        private static (IViewStore<UsersLoggedInInHour>, IDisposable) ClotMongoViewStore()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017/TestDb");
            return (
                new MongoDbViewStore<UsersLoggedInInHour>(mongoClient.GetDatabase("TestDb"), viewName => viewName),
                new NoDisposable());
        }

        private static (IViewStore<UsersLoggedInInHour>, IDisposable) CachedMongoViewStore()
        {
            return ViewStoreCacheFactory<UsersLoggedInInHour>.New()
                .WithCacheItemExpirationPeriod(TimeSpan.FromHours(1))
                .WithCacheDrainPeriod(TimeSpan.FromMilliseconds(1000))
                .WithCacheDrainBatchSize(500)
                .For(ClotMongoViewStore().Item1)
                .Build();
        }
    }
}