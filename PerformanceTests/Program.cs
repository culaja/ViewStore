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

        private static void ExecuteTest(Tuple<IViewStore, IDisposable> tuple)
        {
            var sw = new Stopwatch();
            sw.Start();
            using (tuple.Item2)
            {
                var startGlobalVersion = tuple.Item1.ReadGlobalVersion<UsersLoggedInInHour>() ?? 1;
                Console.WriteLine($"Start global version: {startGlobalVersion}");
                var documentIdCounter = startGlobalVersion % 100;
                
                for (var nextGlobalVersion = startGlobalVersion; nextGlobalVersion <= 2190000; nextGlobalVersion++)
                {
                    var documentId = new UsersLoggedInInHourId(documentIdCounter.ToString());
                    if (nextGlobalVersion % 100 == 0)
                    {
                        documentIdCounter++;
                    }
                    
                    var optionalView = tuple.Item1.Read<UsersLoggedInInHour>(documentId);
                    if (optionalView == null)
                    {
                        optionalView = new UsersLoggedInInHour(documentId.ToString(), nextGlobalVersion);
                    }
                    
                    tuple.Item1.Save(documentId, optionalView.Increment(nextGlobalVersion));
                }
            }
            sw.Stop();
            
            Console.WriteLine($"[{tuple.Item1.GetType().Name}] {sw.ElapsedMilliseconds / 1000m}");
        }

        private class NoDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        private static Tuple<IViewStore, IDisposable> ClotMongoViewStore()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017/TestDb");
            return new Tuple<IViewStore, IDisposable>(
                new MongoDbViewStore(mongoClient.GetDatabase("TestDb"), viewName => viewName),
                new NoDisposable());
        }

        private static Tuple<IViewStore, IDisposable> CachedMongoViewStore()
        {
            return ViewStoreCacheFactory.New()
                .WithCacheItemExpirationPeriod(TimeSpan.FromHours(1))
                .WithCacheDrainPeriod(TimeSpan.FromMilliseconds(1000))
                .WithCacheDrainBatchSize(500)
                .For(ClotMongoViewStore().Item1)
                .Build();
        }
    }
}