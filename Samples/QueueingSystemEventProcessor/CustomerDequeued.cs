using System;

namespace QueueingSystemEventProcessor
{
    public sealed class CustomerDequeued : IEvent
    {
        public string QueueName { get; }
        public int TicketNumber { get; }
        public DateTime DequeuedTime { get; }
        public string ViewId => "SingleView";

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