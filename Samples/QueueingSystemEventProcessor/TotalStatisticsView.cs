using ViewStore.Abstractions;

namespace QueueingSystemEventProcessor
{
    internal sealed class TotalStatisticsView : IView
    {
        public long EnqueuedCustomers { get; }
        public long DequeuedCustomers { get; }
        public long AssignedCustomers { get; }
        public long ServedCustomers { get; }

        public TotalStatisticsView(
            long enqueuedCustomers,
            long dequeuedCustomers,
            long assignedCustomers,
            long servedCustomers)
        {
            EnqueuedCustomers = enqueuedCustomers;
            DequeuedCustomers = dequeuedCustomers;
            AssignedCustomers = assignedCustomers;
            ServedCustomers = servedCustomers;
        }

        public static TotalStatisticsView New() => new(0, 0, 0, 0);

        public TotalStatisticsView Apply(IEvent e)
        {
            switch (e)
            {
                case CustomerAssigned customerAssigned:
                    return new(
                        EnqueuedCustomers,
                        DequeuedCustomers, 
                        AssignedCustomers + 1,
                        ServedCustomers);
                case CustomerDequeued customerDequeued:
                    return new(
                        EnqueuedCustomers,
                        DequeuedCustomers + 1, 
                        AssignedCustomers,
                        ServedCustomers);
                case CustomerEnqueued customerEnqueued:
                    return new(
                        EnqueuedCustomers + 1,
                        DequeuedCustomers, 
                        AssignedCustomers,
                        ServedCustomers);
                case CustomerServed customerServed:
                    return new(
                        EnqueuedCustomers,
                        DequeuedCustomers, 
                        AssignedCustomers,
                        ServedCustomers + 1);
                default:
                    return this;
            }
        }
    }
}