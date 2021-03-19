using ViewStore.Abstractions;

namespace ViewStore.WriteThroughCache
{
    internal sealed class ViewMetaData : IView
    {
        public const string MetaDataId = nameof(MetaDataId);
        
        public string Id { get; }
        public GlobalVersion GlobalVersion { get; }

        public ViewMetaData(string id, GlobalVersion globalVersion)
        {
            Id = id;
            GlobalVersion = globalVersion;
        }

        public static ViewMetaData Of(GlobalVersion globalVersion) =>
            new(MetaDataId, globalVersion);
    }
}