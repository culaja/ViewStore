using Abstractions;

namespace PerformanceTests
{
    internal sealed class UsersLoggedInInHour : IView
    {
        public string Id { get; }
        public int NumberOfLoggedIns { get; private set; }

        public UsersLoggedInInHour(string id, int numberOfLoggedIns = 0)
        {
            Id = id;
            NumberOfLoggedIns = numberOfLoggedIns;
        }

        public UsersLoggedInInHour Increment()
        {
            NumberOfLoggedIns++;
            return this;
        }
    }
}