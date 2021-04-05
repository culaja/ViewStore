using System;

namespace WriteBehindCacheTestApp
{
    public sealed class CustomerAssigned
    {
        public string QueueName { get; }
        public int TicketNumber { get; }
        public string CounterName { get; }
        public DateTime AssignedTime { get; }

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