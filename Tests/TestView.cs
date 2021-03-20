using MongoDB.Bson.Serialization.Attributes;
using ViewStore.Abstractions;

namespace ViewStore.Tests
{
    [BsonIgnoreExtraElements]
    public sealed class TestView : IView
    {
        public static readonly ViewEnvelope TestViewEnvelope1 = new("1", new TestView(1), GlobalVersion.Of(0 ,1));
        public static readonly ViewEnvelope TestViewEnvelope2 = new("2", new TestView(2), GlobalVersion.Of(0, 2));

        public int Number { get; }

        public TestView(int number)
        {
            Number = number;
        }

        private bool Equals(TestView other)
        {
            return Number == other.Number;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is TestView other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Number;
        }

        public TestView Increment() => new(Number + 1);
    }
}