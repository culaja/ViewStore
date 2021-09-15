using System;

namespace WriteBehindCacheTestApp
{
    public sealed class CustomerDequeued
    {
        public string QueueName { get; }
        public int TicketNumber { get; }
        public DateTime DequeuedTime { get; }

        public CustomerDequeued(
            string queueName,
            int ticketNumber,
            DateTime dequeuedTime)
        {
            QueueName = queueName;
            TicketNumber = ticketNumber;
            DequeuedTime = dequeuedTime;
        }

        public override string ToString()
        {
            return $"[{nameof(CustomerDequeued)}] {nameof(QueueName)}: {QueueName}, {nameof(TicketNumber)}: {TicketNumber}, {nameof(DequeuedTime)}: {DequeuedTime}";
        }
    }
}