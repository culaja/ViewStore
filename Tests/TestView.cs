using Abstractions;
using MongoDB.Bson.Serialization.Attributes;

namespace Tests
{
    [BsonIgnoreExtraElements]
    public sealed class TestView : IView
    {
        public static TestView TestView1 = new TestView(1, 0);
        public static TestView TestView2 = new TestView(2, 0);
        
        public int Number { get; private set; }
        public long GlobalVersion { get; }

        public TestView(int number, long globalVersion)
        {
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