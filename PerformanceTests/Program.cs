using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Abstractions;
using Cache;
using MongoDB.Driver;
using Stores.MongoDb;
using Tests;

namespace PerformanceTests
{
    class Program
    {
        static void Main()
        {
            //await ExecuteTest(ClotMongoViewStore());
            ExecuteTest(CachedMongoViewStore());
        }

        private static void ExecuteTest(Tuple<IViewStore, IDisposable> tuple)
        {
            var sw = new Stopwatch();
            sw.Start();
            using (tuple.Item2)
            {
                for (var i = 0; i < 21900; i++)
                {
                    tuple.Item1.Save(new TestViewId(i.ToString()), new TestView("N/A"));
                    for (var j = 0; j < 100; ++j)
                    {
                        tuple.Item1.Read<TestView>(new TestViewId(i.ToString()));
                        tuple.Item1.Save(new TestViewId(i.ToString()), new TestView($"M{j}"));
                    }
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
                .WithCacheDrainBatchSize(500)
                .For(ClotMongoViewStore().Item1)
                .Build();
        }
    }
}