using ViewStore.Abstractions;

namespace ViewStore.MongoDb
{
    internal sealed class ViewEnvelopeInternal
    {
        public string Id { get; }
        public IView View { get; }
        public long GlobalVersion { get; }
        public MetaData MetaData { get; }

        public ViewEnvelopeInternal(
            string id,
            IView view,
            long globalVersion,
            MetaData metaData)
        {
            Id = id;
            View = view;
            GlobalVersion = globalVersion;
            MetaData = metaData;
        }

        public ViewEnvelope ToViewEnvelope() => new(
            Id,
            View,
            Abstractions.GlobalVersion.Of(GlobalVersion),
            MetaData);

        public static implicit operator ViewEnvelope(ViewEnvelopeInternal @internal) =>
            @internal.ToViewEnvelope();
    }
}