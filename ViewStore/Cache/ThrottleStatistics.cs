using System;

namespace ViewStore.Cache
{
    public sealed class ThrottleStatistics
    {
        public long CacheCount { get; }
        public int ThrottleAfterCacheCount { get; }
        public TimeSpan ThrottlePeriod { get; }

        public ThrottleStatistics(
            long cacheCount,
            int throttleAfterCacheCount,
            TimeSpan throttlePeriod)
        {
            CacheCount = cacheCount;
            ThrottleAfterCacheCount = throttleAfterCacheCount;
            ThrottlePeriod = throttlePeriod;
        }

        public override string ToString()
        {
            return $"{nameof(CacheCount)}: {CacheCount}, {nameof(ThrottleAfterCacheCount)}: {ThrottleAfterCacheCount}, {nameof(ThrottlePeriod)}: {ThrottlePeriod}";
        }
    }
}