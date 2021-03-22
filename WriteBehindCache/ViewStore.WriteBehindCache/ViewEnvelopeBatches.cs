using System.Collections;
using System.Collections.Generic;
using ViewStore.Abstractions;

namespace ViewStore.WriteBehindCache
{
    internal sealed class ViewEnvelopeBatches : IReadOnlyList<IReadOnlyList<ViewEnvelope>>
    {
        private readonly IReadOnlyList<IReadOnlyList<ViewEnvelope>> _batches;

        public ViewEnvelopeBatches(IEnumerable<ViewEnvelope> viewEnvelopes, int batchSize)
        {
            _batches = Batch(
                viewEnvelopes,
                batchSize,
                out var largestGlobalVersion,
                out var countOfAllViews);
            LargestGlobalVersion = largestGlobalVersion;
            CountOfAllViewEnvelopes = countOfAllViews;
        }

        public IEnumerator<IReadOnlyList<ViewEnvelope>> GetEnumerator() => _batches.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _batches.GetEnumerator();

        public int Count => _batches.Count;
        public GlobalVersion? LargestGlobalVersion { get; }
        public int CountOfAllViewEnvelopes { get; }

        public IReadOnlyList<ViewEnvelope> this[int index] => _batches[index];
        
        // Copied from https://github.com/morelinq/MoreLINQ
        private static IReadOnlyList<IReadOnlyList<ViewEnvelope>> Batch(
            IEnumerable<ViewEnvelope> source, 
            int batchSize,
            out GlobalVersion? largestGlobalVersion,
            out int countOfAllViews)
        {
            List<ViewEnvelope>? bucket = null;
            largestGlobalVersion = null;
            countOfAllViews = 0;

            var resultList = new List<List<ViewEnvelope>>();
            foreach (var item in source)
            {
                if (bucket == null)
                    bucket = new List<ViewEnvelope>(batchSize);

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