using System.Collections;
using System.Collections.Generic;
using ViewStore.Abstractions;

namespace ViewStore.Cache
{
    internal sealed class DeletedViewEnvelopeBatches : IReadOnlyList<IReadOnlyList<DeletedViewEnvelope>>
    {
        private readonly IReadOnlyList<IReadOnlyList<DeletedViewEnvelope>> _batches;
        public GlobalVersion? LargestGlobalVersion { get; }
        public int CountOfAllViewEnvelopes { get; }


        public DeletedViewEnvelopeBatches(IEnumerable<DeletedViewEnvelope> deletedViewEnvelopes, int batchSize)
        {
            _batches = Batch(
                deletedViewEnvelopes,
                batchSize,
                out var largestGlobalVersion,
                out var countOfAllViews);
            LargestGlobalVersion = largestGlobalVersion;
            CountOfAllViewEnvelopes = countOfAllViews;
        }

        public IEnumerator<IReadOnlyList<DeletedViewEnvelope>> GetEnumerator() => _batches.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _batches.Count;

        public IReadOnlyList<DeletedViewEnvelope> this[int index] => _batches[index];
        
        // Copied from https://github.com/morelinq/MoreLINQ
        private static IReadOnlyList<IReadOnlyList<DeletedViewEnvelope>> Batch(
            IEnumerable<DeletedViewEnvelope> source, 
            int batchSize,
            out GlobalVersion? largestGlobalVersion,
            out int countOfAllViews)
        {
            List<DeletedViewEnvelope>? bucket = null;
            largestGlobalVersion = null;
            countOfAllViews = 0;

            var resultList = new List<List<DeletedViewEnvelope>>();
            foreach (var item in source)
            {
                if (bucket == null)
                    bucket = new List<DeletedViewEnvelope>(batchSize);

                bucket.Add(item);
                largestGlobalVersion = 
                    largestGlobalVersion < item.GlobalVersion ? item.GlobalVersion : largestGlobalVersion
                                                                                     ?? item.GlobalVersion;

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