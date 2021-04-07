using System;

namespace QueueingSystemEventProcessor
{
    public sealed class CustomerAssigned : IEvent
    {
        public string QueueName { get; }
        public int TicketNumber { get; }
        public string CounterName { get; }
        public DateTime AssignedTime { get; }
        
        public string ViewId => "SingleView";

        public CustomerAssigned(
            string queueName,
            int ticketNumber,
            string counterName,
            DateTime assignedTime)
        {
            QueueName = queueName;
            TicketNumber = ticketNumber;
            CounterName = counterName;
            AssignedTime = assignedTime;
        }

        public override string ToString()
        {
            return $"[{nameof(CustomerAssigned)}] {nameof(QueueName)}: {QueueName}, {nameof(TicketNumber)}: {TicketNumber}, {nameof(CounterName)}: {CounterName}, {nameof(AssignedTime)}: {AssignedTime}";
        }
    }
}