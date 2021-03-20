using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using ViewStore.Abstractions;

namespace ViewStore.MongoDb
{
    public class GlobalVersionSerializer : SerializerBase<GlobalVersion>
    {
        public override GlobalVersion Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            context.Reader.ReadStartDocument();
            context.Reader.ReadName();
            var part1 = context.Reader.ReadInt64();
            context.Reader.ReadName();
            var part2 = context.Reader.ReadInt64();
            context.Reader.ReadEndDocument();
            return GlobalVersion.Of(part1, part2);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, GlobalVersion value)
        {
            context.Writer.WriteStartDocument();
            context.Writer.WriteName("Part1");
            context.Writer.WriteInt64(value.Part1);
            context.Writer.WriteName("Part2");
            context.Writer.WriteInt64(value.Part2);
            context.Writer.WriteEndDocument();
        }
    }
}