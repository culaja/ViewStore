using System;
using System.Collections.Generic;
using System.Diagnostics;
using MongoDB.Driver;
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
            var mongoViewStore = new MongoDbViewStore(mongoDatabase, nameof(StoryLikesPerHour));
            
            var viewStoreCache = ViewStoreCacheFactory.New()
                .WithCacheDrainPeriod(TimeSpan.FromMilliseconds(1000))
                .WithCacheDrainBatchSize(500)
                .For(mongoViewStore)
                .UseCallbackWhenDrainFinished(Console.WriteLine)
                .Build();

            var generatedEvents = GenerateEventsFor(
                1,
                new DateTime(2021, 1, 1),
                new DateTime(2022, 1, 1),
                TimeSpan.FromSeconds(2));

            var sw = new Stopwatch();
            sw.Start();
            using (viewStoreCache)
            {
                foreach (var storyIsLiked in generatedEvents)
                {
                    var view = viewStoreCache.Read<StoryLikesPerHour>(storyIsLiked.StoryLikesPerHourId)
                        ?? new StoryLikesPerHour(storyIsLiked.StoryLikesPerHourId, storyIsLiked.GlobalVersion, 0L);

                    if (view.GlobalVersion < storyIsLiked.GlobalVersion)
                    {
                        view.Apply();
                        viewStoreCache.Save(view);
                    }
                }
            }
            sw.Stop();
            
            Console.WriteLine($"Elapsed: {sw.Elapsed.TotalSeconds}");
        }

        private static IEnumerable<StoryIsLiked> GenerateEventsFor(
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
                    yield return new StoryIsLiked($"Story{i}", startDate, globalVersion++);
                }
            }
        }
    }
}