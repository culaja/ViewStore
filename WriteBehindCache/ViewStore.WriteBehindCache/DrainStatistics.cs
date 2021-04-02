using System;

namespace ViewStore.WriteBehindCache
{
    public sealed class DrainStatistics
    {
        public TimeSpan Duration { get; }
        public int ViewCount { get; }
        public int RetryCount { get; }

        internal DrainStatistics(
            TimeSpan duration,
            int viewCount,
            int retryCount)
        {
            Duration = duration;
            ViewCount = viewCount;
            RetryCount = retryCount;
        }

        public override string ToString()
        {
            return $"{nameof(Duration)}: {Duration}, {nameof(ViewCount)}: {ViewCount}, {nameof(RetryCount)}: {RetryCount}";
        }
    }
}