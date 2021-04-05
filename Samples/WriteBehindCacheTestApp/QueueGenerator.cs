using System;
using System.Collections.Generic;
// ReSharper disable PossibleLossOfFraction

namespace WriteBehindCacheTestApp
{
    internal sealed class QueueGenerator
    {
        private readonly int _numberOfCountersPerQueue;
        private readonly TimeSpan _timeSpanBetweenCustomers;
        private DateTime _currentTime;

        public QueueGenerator(
            string queueName,
            int numberOfCountersPerQueue,
            int numberOfCustomersPerHourPerCounter,
            DateTime startTime)
        {
            QueueName = queueName;
            _numberOfCountersPerQueue = numberOfCountersPerQueue;
            _timeSpanBetweenCustomers = TimeSpan.FromSeconds(3600 / numberOfCustomersPerHourPerCounter);
            _currentTime = startTime;
        }

        public string QueueName { get; set; }

        public IEnumerable<object> QueueEvents()
        {
            var ticketNumber = 0;
            while (true)
            {
                for (var i = 1; i <= _numberOfCountersPerQueue; ++i)
                {
                    var delta = _timeSpanBetweenCustomers / 4;
                    yield return new CustomerEnqueued(QueueName, ++ticketNumber, _currentTime);
                    yield return new CustomerDequeued(QueueName, ticketNumber, _currentTime.Add(delta));
                    yield return new CustomerAssigned(QueueName, ticketNumber, $"Counter{i}", _currentTime.Add(delta*2));
                    yield return new CustomerServed(QueueName, ticketNumber, $"Counter{i}", _currentTime.Add(delta*3));
                    
                }
                _currentTime += _timeSpanBetweenCustomers;
            }
        }
    }
}