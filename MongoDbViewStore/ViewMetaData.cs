namespace Stores.MongoDb
{
    internal sealed class ViewMetaData
    {
        public const string VIewMetaDataId = nameof(VIewMetaDataId);
        
        public string Id { get; }
        public long LastKnownPosition { get; }

        public ViewMetaData(string id, long lastKnownPosition)
        {
            Id = id;
            LastKnownPosition = lastKnownPosition;
        }

        public static ViewMetaData ViewMetaDataFrom(long lastKnownGlobalVersion) =>
            new ViewMetaData(VIewMetaDataId, lastKnownGlobalVersion);
    }
}