using MongoDB.Bson.Serialization.Attributes;
using ViewStore.Abstractions;

namespace ViewStore.Tests
{
    [BsonIgnoreExtraElements]
    public sealed class TestView : IView
    {
        public static TestView TestView1 = new TestView("1", 1, 0);
        public static TestView TestView2 = new TestView("2", 2, 0);

        public string Id { get; }
        public int Number { get; private set; }
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
            Number++;
            return this;
        }
    }
}