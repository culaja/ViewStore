using ViewStore.Abstractions;

namespace ViewStore.Cache
{
    public static class ViewMetaData
    {
        public const string MetaDataId = "28963e0b-b51a-459b-be9d-c5ea1aa6f843";

        public static ViewEnvelope MetaDataEnvelopeFor(GlobalVersion globalVersion) =>
            new(
                "28963e0b-b51a-459b-be9d-c5ea1aa6f843", 
                null!,
                globalVersion,
                MetaData.New());
    }
}