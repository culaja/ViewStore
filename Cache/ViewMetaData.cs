using Abstractions;

namespace Cache
{
    internal sealed class ViewMetaData : IView
    {
        public const string MetaDataId = nameof(MetaDataId);
        
        public string Id { get; }
        public long GlobalVersion { get; }

        public ViewMetaData(string id, long globalVersion)
        {
            Id = id;
            GlobalVersion = globalVersion;
        }

        public static ViewMetaData Of(long globalVersion) =>
            new(MetaDataId, globalVersion);
    }
}