using System;

namespace WriteBehindCacheTestApp
{
    public sealed class CustomerEnqueued
    {
        public string QueueName { get; }
        public int TicketNumber { get; }
        public DateTime EnqueuedTime { get; }

        public CustomerEnqueued(
            string queueName,
            int ticketNumber,
            DateTime enqueuedTime)
        {
            QueueName = queueName;
            TicketNumber = ticketNumber;
            EnqueuedTime = enqueuedTime;
        }

        public override string ToString()
        {
            return $"[{nameof(CustomerEnqueued)}] {nameof(QueueName)}: {QueueName}, {nameof(TicketNumber)}: {TicketNumber}, {nameof(EnqueuedTime)}: {EnqueuedTime}";
        }
    }
}