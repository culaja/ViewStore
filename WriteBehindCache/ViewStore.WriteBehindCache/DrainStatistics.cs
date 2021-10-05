using System;

namespace ViewStore.WriteBehindCache
{
    public sealed class DrainStatistics
    {
        public TimeSpan Duration { get; }
        public int AddedOrUpdatedViewCount { get; }
        public int AddedOrUpdatedRetryCount { get; }
        public int DeletedViewCount { get; }
        public int DeletedRetryCount { get; }
        
        public int Count => AddedOrUpdatedRetryCount + DeletedViewCount;
        public bool HasAnyItems => Count > 0;

        internal DrainStatistics(
            TimeSpan duration,
            int addedOrUpdatedViewCount,
            int addedOrUpdatedRetryCount,
            int deletedViewCount,
            int deletedRetryCount)
        {
            Duration = duration;
            AddedOrUpdatedViewCount = addedOrUpdatedViewCount;
            AddedOrUpdatedRetryCount = addedOrUpdatedRetryCount;
            DeletedViewCount = deletedViewCount;
            DeletedRetryCount = deletedRetryCount;
        }

        public override string ToString()
        {
            return $"{nameof(Duration)}: {Duration}, {nameof(AddedOrUpdatedViewCount)}: {AddedOrUpdatedViewCount}, {nameof(AddedOrUpdatedRetryCount)}: {AddedOrUpdatedRetryCount}, {nameof(DeletedViewCount)}: {DeletedViewCount}, {nameof(DeletedRetryCount)}: {DeletedRetryCount}";
        }
    }
}