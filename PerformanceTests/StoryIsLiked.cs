using System;

namespace ViewStore.PerformanceTests
{
    internal sealed class StoryIsLiked
    {
        private static DateTime _utc2020 = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        public string StoryName { get; }
        public DateTime Timestamp { get; }
        public long GlobalVersion { get; }

        public string StoryLikesPerHourId => $"{StoryName}_{TimestampHour}";
        public long TimestampHour => (long) (Timestamp - _utc2020).TotalHours;

        public StoryIsLiked(
            string storyName,
            DateTime timestamp,
            long globalVersion)
        {
            StoryName = storyName;
            Timestamp = timestamp;
            GlobalVersion = globalVersion;
        }
    }
}