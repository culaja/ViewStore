using Abstractions;

namespace PerformanceTests
{
    internal sealed class UsersLoggedInInHourId : IViewId
    {
        public string Id { get; }

        public UsersLoggedInInHourId(string id)
        {
            Id = id;
        }

        public override string ToString() => Id;

        private bool Equals(UsersLoggedInInHourId other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is UsersLoggedInInHourId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }
}