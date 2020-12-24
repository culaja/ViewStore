using Abstractions;
using MongoDB.Bson.Serialization.Attributes;

namespace Tests
{
    [BsonIgnoreExtraElements]
    public sealed class TestView : IView
    {
        public static TestView TestView1 = new TestView(1);
        public static TestView TestView2 = new TestView(2);
        
        public int Number { get; private set; }

        public TestView(int number)
        {
            Number = number;
        }

        public TestView IncrementNumber()
        {
            Number++;
            return this;
        }
    }
}