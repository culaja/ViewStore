using System;
using Abstractions;
using MongoDB.Bson.Serialization.Attributes;

namespace Tests
{
    [BsonIgnoreExtraElements]
    public sealed class TestView : IView, IEquatable<TestView>
    {
        public static TestView TestView1 = new TestView(nameof(TestView1));
        public static TestView TestView2 = new TestView(nameof(TestView2));
        public string SomeInfo { get; }

        public TestView(string someInfo)
        {
            SomeInfo = someInfo;
        }
        
        public override string ToString() => SomeInfo;

        public bool Equals(TestView? other)
        {
            if (other != null)
            {
                return SomeInfo == other.SomeInfo;
            }

            return false;
        }
    }
}