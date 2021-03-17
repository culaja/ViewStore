using MongoDB.Bson.Serialization.Attributes;
using ViewStore.Abstractions;

namespace ViewStore.MongoDb
{
    [BsonIgnoreExtraElements]
    internal sealed class View : IView
    {
        public View(string id, long globalVersion)
        {
            Id = id;
            GlobalVersion = globalVersion;
        }

        public string Id { get; }
        public long GlobalVersion { get; }
    }
}