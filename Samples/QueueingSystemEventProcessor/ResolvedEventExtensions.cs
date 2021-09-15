using System;
using System.Text;
using EventStore.Client;
using Newtonsoft.Json;
using ViewStore.Abstractions;

namespace QueueingSystemEventProcessor
{
    public static class ResolvedEventExtensions
    {
        public static IEvent ToEvent(this ResolvedEvent resolvedEvent)
        {
            var eventData = Encoding.ASCII.GetString(resolvedEvent.Event.Data.Span);
            return (IEvent) JsonConvert.DeserializeObject(
                eventData, 
                Type.GetType($"{nameof(QueueingSystemEventProcessor)}.{resolvedEvent.Event.EventType}"));
        }

        public static GlobalVersion ToGlobalVersion(this ResolvedEvent resolvedEvent) =>
            GlobalVersion.FromUlong(
                resolvedEvent.OriginalPosition.Value.CommitPosition,
                resolvedEvent.OriginalPosition.Value.PreparePosition);
    }
}