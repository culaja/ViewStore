using System.Collections;
using System.Collections.Generic;

namespace ViewStore.Cache;

internal sealed class DeletedViewRecordBatches : IReadOnlyList<IReadOnlyList<DeletedViewRecord>>
{
    private readonly IReadOnlyList<IReadOnlyList<DeletedViewRecord>> _batches;
    public long? LargestGlobalVersion { get; }
    public int CountOfAllViewEnvelopes { get; }


    public DeletedViewRecordBatches(IEnumerable<DeletedViewRecord> deletedViewEnvelopes, int batchSize)
    {
        _batches = Batch(
            deletedViewEnvelopes,
            batchSize,
            out var largestGlobalVersion,
            out var countOfAllViews);
        LargestGlobalVersion = largestGlobalVersion;
        CountOfAllViewEnvelopes = countOfAllViews;
    }

    public IEnumerator<IReadOnlyList<DeletedViewRecord>> GetEnumerator() => _batches.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _batches.Count;

    public IReadOnlyList<DeletedViewRecord> this[int index] => _batches[index];
        
    // Copied from https://github.com/morelinq/MoreLINQ
    private static IReadOnlyList<IReadOnlyList<DeletedViewRecord>> Batch(
        IEnumerable<DeletedViewRecord> source, 
        int batchSize,
        out long? largestGlobalVersion,
        out int countOfAllViews)
    {
        List<DeletedViewRecord>? bucket = null;
        largestGlobalVersion = null;
        countOfAllViews = 0;

        var resultList = new List<List<DeletedViewRecord>>();
        foreach (var item in source)
        {
            if (bucket == null)
                bucket = new List<DeletedViewRecord>(batchSize);

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