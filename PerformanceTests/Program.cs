using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Caching;
using MongoDB.Driver;
using ViewStore.MongoDb;
using ViewStore.ReadThroughCache;
using ViewStore.WriteThroughCache;

namespace ViewStore.PerformanceTests
{
    class Program
    {
        static void Main()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017/TestDb");
            var mongoDatabase = mongoClient.GetDatabase("TestDb");
            var mongoViewStore = new MongoDbViewStore(mongoDatabase, nameof(StoryLikesPerHour));
            
            var writeThroughViewStoreCache = ViewStoreCacheFactory.New()
                .WithCacheDrainPeriod(TimeSpan.FromSeconds(1))
                .WithCacheDrainBatchSize(500)
                .For(mongoViewStore)
                .UseCallbackWhenDrainFinished(count => Console.WriteLine($"Bulk write count: {count}"))
                .Build();

            var generatedEvents = GenerateEventsFor(
                writeThroughViewStoreCache.ReadLastKnownPosition(),
                100,
                new DateTime(2021, 1, 1),
                new DateTime(2022, 1, 1),
                TimeSpan.FromSeconds(20));

            var readThroughCache = new ReadThroughViewStoreCache(
                MemoryCache.Default,
                TimeSpan.FromSeconds(10),
                writeThroughViewStoreCache);

            var sw = new Stopwatch();
            sw.Start();
            var totalNumberOfEvents = 0L;
            using (writeThroughViewStoreCache)
            {
                foreach (var storyIsLiked in generatedEvents)
                {
                    var view = readThroughCache.Read<StoryLikesPerHour>(storyIsLiked.StoryLikesPerHourId)
                               ?? new StoryLikesPerHour(storyIsLiked.StoryLikesPerHourId, -1L, 0L);

                    if (view.GlobalVersion < storyIsLiked.GlobalVersion)
                    {
                        view.Apply(storyIsLiked);
                        readThroughCache.Save(view);
                    }

                    totalNumberOfEvents++;
                }
            }
            sw.Stop();
            
            Console.WriteLine($"Elapsed: {sw.Elapsed.TotalSeconds}");
            Console.WriteLine($"Total number of events: {totalNumberOfEvents}");
        }

        private static IEnumerable<StoryIsLiked> GenerateEventsFor(
            long? globalPosition,
            int storyCount,
            DateTime startDate,
            DateTime endDate,
            TimeSpan delta)
        {
            var globalVersion = 0L;
            for (var timestamp = startDate; timestamp < endDate; timestamp += delta)
            {
                for (var i = 0; i < storyCount; ++i)
                {
                    globalVersion++;
                    
                    if (globalPosition == null || globalPosition < globalVersion)
                    {
                        yield return new StoryIsLiked($"Story{i}", timestamp, globalVersion);
                    }
                }
            }
        }
    }
}