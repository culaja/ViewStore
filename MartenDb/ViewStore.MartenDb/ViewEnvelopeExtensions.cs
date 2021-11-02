using ViewStore.Abstractions;

namespace ViewStore.MartenDb
{
    internal static class ViewEnvelopeExtensions
    {
        public static ViewEnvelopeInternal ToViewEnvelopeInternal(this ViewEnvelope viewEnvelope) =>
            new(viewEnvelope.Id, viewEnvelope.View, viewEnvelope.GlobalVersion.Value, viewEnvelope.MetaData);
    }
}