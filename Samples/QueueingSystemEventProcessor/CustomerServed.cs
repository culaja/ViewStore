using System;

namespace QueueingSystemEventProcessor
{
    public sealed class CustomerServed : IEvent
    {
        public string QueueName { get; }
        public int TicketNumber { get; }
        public string CounterName { get; }
        public DateTime ServedTime { get; }
        
        public string ViewId => "SingleView";

        public CustomerServed(
            string queueName,
            int ticketNumber,
            string counterName,
            DateTime servedTime)
        {
            QueueName = queueName;
            TicketNumber = ticketNumber;
            CounterName = counterName;
            ServedTime = servedTime;
        }

        public override string ToString()
        {
            return $"[{nameof(CustomerServed)}] {nameof(QueueName)}: {QueueName}, {nameof(TicketNumber)}: {TicketNumber}, {nameof(CounterName)}: {CounterName}, {nameof(ServedTime)}: {ServedTime}";
        }
    }
}