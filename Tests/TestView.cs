using System;
using MongoDB.Bson.Serialization.Attributes;
using ViewStore.Abstractions;

namespace ViewStore.Tests
{
    [BsonIgnoreExtraElements]
    public sealed class TestView : IView
    {
        public static readonly TestView TestView1 = new("1", 1, 1);
        public static readonly TestView TestView2 = new("2", 2, 2);

        public string Id { get; }
        public int Number { get; }
        public long GlobalVersion { get; }

        public TestView(
            string id,
            int number,
            long globalVersion)
        {
            Id = id;
            Number = number;
            GlobalVersion = globalVersion;
        }

        public TestView IncrementNumber()
        {
            return new(Id, Number + 1, GlobalVersion);
        }

        private bool Equals(TestView other)
        {
            return Id == other.Id && Number == other.Number && GlobalVersion == other.GlobalVersion;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is TestView other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Number, GlobalVersion);
        }
    }
}