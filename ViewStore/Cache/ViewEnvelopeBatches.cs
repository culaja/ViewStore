using System.Collections;
using System.Collections.Generic;

namespace ViewStore.Cache;

internal sealed class ViewEnvelopeBatches : IReadOnlyList<IReadOnlyList<ViewRecord>>
{
    private readonly IReadOnlyList<IReadOnlyList<ViewRecord>> _batches;

    public ViewEnvelopeBatches(IEnumerable<ViewRecord> viewEnvelopes, int batchSize)
    {
        _batches = Batch(
            viewEnvelopes,
            batchSize,
            out var largestGlobalVersion,
            out var countOfAllViews);
        LargestGlobalVersion = largestGlobalVersion;
        CountOfAllViewEnvelopes = countOfAllViews;
    }

    public IEnumerator<IReadOnlyList<ViewRecord>> GetEnumerator() => _batches.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _batches.GetEnumerator();

    public int Count => _batches.Count;
    public long? LargestGlobalVersion { get; }
    public int CountOfAllViewEnvelopes { get; }

    public IReadOnlyList<ViewRecord> this[int index] => _batches[index];
        
    // Copied from https://github.com/morelinq/MoreLINQ
    private static IReadOnlyList<IReadOnlyList<ViewRecord>> Batch(
        IEnumerable<ViewRecord> source, 
        int batchSize,
        out long? largestGlobalVersion,
        out int countOfAllViews)
    {
        List<ViewRecord>? bucket = null;
        largestGlobalVersion = null;
        countOfAllViews = 0;

        var resultList = new List<List<ViewRecord>>();
        foreach (var item in source)
        {
            if (bucket == null)
                bucket = new List<ViewRecord>(batchSize);

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