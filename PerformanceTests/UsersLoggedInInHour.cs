using Abstractions;

namespace PerformanceTests
{
    internal sealed class UsersLoggedInInHour : IView
    {
        public string Id { get; }
        public long GlobalVersion { get; private set; }
        public int NumberOfLoggedIns { get; private set; }

        public UsersLoggedInInHour(string id, long globalVersion, int numberOfLoggedIns = 0)
        {
            Id = id;
            GlobalVersion = globalVersion;
            NumberOfLoggedIns = numberOfLoggedIns;
        }

        public UsersLoggedInInHour Increment(long nextGlobalVersion)
        {
            GlobalVersion = nextGlobalVersion;
            NumberOfLoggedIns++;
            return this;
        }
    }
}