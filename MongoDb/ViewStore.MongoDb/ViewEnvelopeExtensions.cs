using ViewStore.Abstractions;

namespace ViewStore.MongoDb
{
    internal static class ViewEnvelopeExtensions
    {
        public static ViewEnvelopeInternal ToViewEnvelopeInternal(this ViewEnvelope viewEnvelope) =>
            new(viewEnvelope.Id, viewEnvelope.View, viewEnvelope.GlobalVersion.Value, viewEnvelope.MetaData);
    }
}