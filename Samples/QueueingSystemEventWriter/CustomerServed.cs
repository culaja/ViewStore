using System;

namespace QueueingSystemEventWriter
{
    public sealed class CustomerServed
    {
        public string QueueName { get; }
        public int TicketNumber { get; }
        public string CounterName { get; }
        public DateTime ServedTime { get; }

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