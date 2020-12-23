using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Abstractions;
using Cache;
using MongoDB.Driver;
using Stores.MongoDb;
using Tests;
using static Tests.TestView;

namespace PerformanceTests
{
    class Program
    {
        static async Task Main()
        {
            //await ExecuteTest(ClotMongoViewStore());
            await ExecuteTest(CachedMongoViewStore());
        }

        private static async Task ExecuteTest(Tuple<IViewStore, IDisposable> tuple)
        {
            var sw = new Stopwatch();
            sw.Start();
            using (tuple.Item2)
            {
                for (var i = 0; i < 100000; i++)
                {
                    await tuple.Item1.SaveAsync(new TestViewId(i.ToString()), TestView1);
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
                .WithCacheDrainPeriod(TimeSpan.FromMilliseconds(100))
                .WithCacheDrainBatchSize(200)
                .For(ClotMongoViewStore().Item1)
                .Build();
        }
    }
}