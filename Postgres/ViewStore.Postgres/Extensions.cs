using ViewStore.Abstractions;

namespace ViewStore.Postgres
{
    internal static class Extensions
    {
        public static ViewEnvelopeInternal ToInternal(this ViewEnvelope viewEnvelope) => new(viewEnvelope);
    }
}