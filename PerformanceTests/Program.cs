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
            var mongoClient = new MongoClient("mongodb://kolotree:dagi987@localhost:27017");
            var mongoDatabase = mongoClient.GetDatabase("MES");
            var mongoViewStore = new MongoDbViewStore(mongoDatabase, nameof(StoryLikesPerHour));
            Console.WriteLine(mongoViewStore.ReadLastKnownPosition());
            
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