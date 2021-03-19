using MongoDB.Bson.Serialization.Attributes;
using ViewStore.Abstractions;

namespace ViewStore.MongoDb
{
    [BsonIgnoreExtraElements]
    internal sealed class ViewDto : IView
    {
        public ViewDto(string id, GlobalVersion globalVersion)
        {
            Id = id;
            GlobalVersion = globalVersion;
        }

        public string Id { get; }
        
        [BsonSerializer(typeof(GlobalVersionSerializer))]
        public GlobalVersion GlobalVersion { get; }
    }
}