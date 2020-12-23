using System;
using Abstractions;

namespace Tests
{
    public sealed class TestView : IView, IEquatable<TestView>
    {
        public static TestView TestView1 = new TestView(nameof(TestView1));
        public static TestView TestView2 = new TestView(nameof(TestView2));
        
        private readonly string _id;

        private TestView(string id)
        {
            _id = id;
        }

        public override string ToString() => _id;

        public bool Equals(TestView? other)
        {
            if (other != null)
            {
                return _id == other._id;
            }

            return false;
        }
    }
}