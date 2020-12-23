using System;
using Abstractions;

namespace Tests
{
    public sealed class TestViewId : IViewId, IEquatable<TestViewId>
    {
        public static TestViewId TestViewId1 = new TestViewId(nameof(TestViewId1));
        public static TestViewId TestViewId2 = new TestViewId(nameof(TestViewId2));
        
        private readonly string _id;

        public TestViewId(string id)
        {
            _id = id;
        }

        public override string ToString() => _id;

        public bool Equals(TestViewId? other)
        {
            if (other != null)
            {
                return _id == other._id;
            }

            return false;
        }
    }
}