using ViewStore.Abstractions;

namespace ViewStore.PerformanceTestsnceTests
{
    internal sealed class StoryLikesPerHour : IView
    {
        public string Id { get; }

        public long GlobalVersion { get; }
        public long NumberOfLikes { get; private set; }

        public StoryLikesPerHour(
            string id,
            long globalVersion,
            long numberOfLikes)
        {
            Id = id;
            GlobalVersion = globalVersion;
            NumberOfLikes = numberOfLikes;
        }

        public void Apply()
        {
            NumberOfLikes++;
        }
    }
}