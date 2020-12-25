namespace Stores.MongoDb
{
    internal sealed class ViewMetaData
    {
        public const string VIewMetaDataId = nameof(VIewMetaDataId);
        
        public string Id { get; }
        public long LastKnownGlobalVersion { get; }

        public ViewMetaData(string id, long lastKnownGlobalVersion)
        {
            Id = id;
            LastKnownGlobalVersion = lastKnownGlobalVersion;
        }

        public static ViewMetaData ViewMetaDataFrom(long lastKnownGlobalVersion) =>
            new ViewMetaData(VIewMetaDataId, lastKnownGlobalVersion);
    }
}