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
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _id == other._id;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is TestViewId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }
}