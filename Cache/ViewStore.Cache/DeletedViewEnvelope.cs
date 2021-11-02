using ViewStore.Abstractions;

namespace ViewStore.Cache
{
    internal readonly struct DeletedViewEnvelope
    {
        public string ViewId { get; }
        public GlobalVersion GlobalVersion { get; }

        public DeletedViewEnvelope(
            string viewId,
            GlobalVersion globalVersion)
        {
            ViewId = viewId;
            GlobalVersion = globalVersion;
        }
    }
}