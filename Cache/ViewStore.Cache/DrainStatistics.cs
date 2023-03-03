using System;
using ViewStore.Abstractions;

namespace ViewStore.Cache
{
    public sealed class DrainStatistics
    {
        public TimeSpan Duration { get; }
        public int AddedOrUpdatedViewCount { get; }
        public int AddedOrUpdatedRetryCount { get; }
        public int DeletedViewCount { get; }
        public int DeletedRetryCount { get; }
        public GlobalVersion? LastGlobalVersion { get; }

        public int Count => AddedOrUpdatedViewCount + DeletedViewCount;
        public bool HasAnyItems => Count > 0;

        internal DrainStatistics(
            TimeSpan duration,
            int addedOrUpdatedViewCount,
            int addedOrUpdatedRetryCount,
            int deletedViewCount,
            int deletedRetryCount,
            GlobalVersion? lastGlobalVersion)
        {
            Duration = duration;
            AddedOrUpdatedViewCount = addedOrUpdatedViewCount;
            AddedOrUpdatedRetryCount = addedOrUpdatedRetryCount;
            DeletedViewCount = deletedViewCount;
            DeletedRetryCount = deletedRetryCount;
            LastGlobalVersion = lastGlobalVersion;
        }

        public override string ToString()
        {
            return $"{nameof(Duration)}: {Duration}, {nameof(AddedOrUpdatedViewCount)}: {AddedOrUpdatedViewCount}, {nameof(AddedOrUpdatedRetryCount)}: {AddedOrUpdatedRetryCount}, {nameof(DeletedViewCount)}: {DeletedViewCount}, {nameof(DeletedRetryCount)}: {DeletedRetryCount}, , {nameof(LastGlobalVersion)}: {LastGlobalVersion}";
        }
    }
}