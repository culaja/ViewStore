using System;
using System.Collections.Generic;
using System.Linq;

namespace QueueingSystemEventWriter
{
    internal sealed class QueueingSystemGenerator
    {
        private readonly int _numberOfQueues;
        private readonly int _numberOfCountersPerQueue;
        private readonly int _numberOfCustomersPerHour;
        private readonly DateTime _startTime;

        private readonly Dictionary<string, IEnumerator<object>> _queueEnumerators = new();

        public QueueingSystemGenerator(
            int numberOfQueues,
            int numberOfCountersPerQueue,
            int numberOfCustomersPerHour,
            DateTime startTime)
        {
            _numberOfQueues = numberOfQueues;
            _numberOfCountersPerQueue = numberOfCountersPerQueue;
            _numberOfCustomersPerHour = numberOfCustomersPerHour;
            _startTime = startTime;
        }
        
        public IEnumerable<object> Generate()
        {
            var queueGenerators = Enumerable.Range(1, _numberOfQueues)
                .Select(i => $"Queue {i}")
                .Select(queueName =>
                    new QueueGenerator(
                        queueName,
                        _numberOfCountersPerQueue,
                        _numberOfCustomersPerHour,
                        _startTime))
                .ToList();

            IReadOnlyList<object> generatedEventsInSingleStep;
            while ((generatedEventsInSingleStep = EnumerateSingleStepThrough(queueGenerators)).Count > 0)
            {
                foreach (var @event in generatedEventsInSingleStep)
                {
                    yield return @event;
                }
            }
        }

        private IReadOnlyList<object> EnumerateSingleStepThrough(IReadOnlyList<QueueGenerator> queueGenerators)
        {
            var list = new List<object>();
            foreach (var queueGenerator in queueGenerators)
            {
                if (!_queueEnumerators.TryGetValue(queueGenerator.QueueName, out var enumerator))
                {
                    enumerator = queueGenerator.QueueEvents().GetEnumerator();
                    _queueEnumerators.Add(queueGenerator.QueueName, enumerator);
                }
                
                if (enumerator.MoveNext())
                {
                    list.Add(enumerator.Current);
                }
            }

            return list;
        }
    }
}