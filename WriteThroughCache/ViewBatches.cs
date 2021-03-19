using System.Collections;
using System.Collections.Generic;
using ViewStore.Abstractions;

namespace ViewStore.WriteThroughCache
{
    internal sealed class ViewBatches : IReadOnlyList<IReadOnlyList<IView>>
    {
        private readonly IReadOnlyList<IReadOnlyList<IView>> _batches;

        public ViewBatches(IEnumerable<IView> views, int batchSize)
        {
            _batches = Batch(
                views,
                batchSize,
                out var largestGlobalVersion,
                out var countOfAllViews);
            LargestGlobalVersion = largestGlobalVersion;
            CountOfAllViews = countOfAllViews;
        }

        public IEnumerator<IReadOnlyList<IView>> GetEnumerator() => _batches.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _batches.GetEnumerator();

        public int Count => _batches.Count;
        public GlobalVersion? LargestGlobalVersion { get; }
        public int CountOfAllViews { get; }

        public IReadOnlyList<IView> this[int index] => _batches[index];
        
        // Copied from https://github.com/morelinq/MoreLINQ
        private static IReadOnlyList<IReadOnlyList<IView>> Batch(
            IEnumerable<IView> source, 
            int batchSize,
            out GlobalVersion? largestGlobalVersion,
            out int countOfAllViews)
        {
            List<IView>? bucket = null;
            largestGlobalVersion = null;
            countOfAllViews = 0;

            var resultList = new List<List<IView>>();
            foreach (var item in source)
            {
                if (bucket == null)
                    bucket = new List<IView>(batchSize);

                bucket.Add(item);
                largestGlobalVersion = largestGlobalVersion < item.GlobalVersion ? item.GlobalVersion : largestGlobalVersion;

                if (bucket.Count != batchSize)
                    continue;

                resultList.Add(bucket);
                countOfAllViews += bucket.Count;
                bucket = null;
            }

            // Return the last bucket with all remaining elements
            if (bucket != null && bucket.Count > 0)
            {
                resultList.Add(bucket);
                countOfAllViews += bucket.Count;
            }

            return resultList;
        }
    }
}