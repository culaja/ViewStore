using System.Collections.Concurrent;
using Prometheus;
using ViewStore.Cache;
using ViewStore.Cache.Cache;

namespace ViewStore.Prometheus;

public static class ViewStoreCacheBuilderExtensions
{
    private static readonly ConcurrentBag<Counter> Counters = new();
    
    public static ViewStoreCacheBuilder AddMetrics(this ViewStoreCacheBuilder viewStoreCacheBuilder, string namePrefix)
    {
        var durationHistogram = Metrics.CreateHistogram($"{namePrefix}:DurationInMs", "View Store Cache drain duration into the final view store");
        var totalDrainedViewCounter = Metrics.CreateCounter($"{namePrefix}:TotalDrainedViews", "Count of added, updated or deleted views drained into final view store");
        var addedOrUpdatedViewCounter = Metrics.CreateCounter($"{namePrefix}:AddedOrUpdatedViews", "Count of added or updated views drained into final view store");
        var addedOrUpdatedRetryCounter = Metrics.CreateCounter($"{namePrefix}:AddedOrUpdatedRetryCount", "Number of retries when attempting to add or update views in final view store");
        var deletedViewCounter = Metrics.CreateCounter($"{namePrefix}:DeletedViews", "Count of deleted views drained into final view store");
        var deletedRetryCounter = Metrics.CreateCounter($"{namePrefix}:DeletedRetryCount", "Number of retries when attempting to delete views in final view store");
        
        Counters.Add(totalDrainedViewCounter);
        Counters.Add(addedOrUpdatedViewCounter);
        Counters.Add(addedOrUpdatedRetryCounter);
        Counters.Add(deletedViewCounter);
        Counters.Add(deletedRetryCounter);

        void OnNewDrainStatistics(DrainStatistics drainStatistics)
        {
            durationHistogram.Observe(drainStatistics.Duration.TotalMilliseconds);
            totalDrainedViewCounter.Inc(drainStatistics.Count);
            addedOrUpdatedViewCounter.Inc(drainStatistics.AddedOrUpdatedViewCount);
            addedOrUpdatedRetryCounter.Inc(drainStatistics.AddedOrUpdatedRetryCount);
            deletedViewCounter.Inc(drainStatistics.DeletedViewCount);
            deletedRetryCounter.Inc(drainStatistics.DeletedRetryCount);
        }

        viewStoreCacheBuilder.UseCallbackWhenDrainFinished(OnNewDrainStatistics);

        return viewStoreCacheBuilder;
    }
}