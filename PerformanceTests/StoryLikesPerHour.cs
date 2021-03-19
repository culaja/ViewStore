using ViewStore.Abstractions;

namespace ViewStore.PerformanceTests
{
    internal sealed class StoryLikesPerHour : IView
    {
        public string Id { get; }

        public GlobalVersion GlobalVersion { get; private set; }
        public long NumberOfLikes { get; private set; }

        public StoryLikesPerHour(
            string id,
            GlobalVersion globalVersion,
            long numberOfLikes)
        {
            Id = id;
            GlobalVersion = globalVersion;
            NumberOfLikes = numberOfLikes;
        }

        public void Apply(StoryIsLiked storyIsLiked)
        {
            NumberOfLikes++;
            GlobalVersion = GlobalVersion.Of(storyIsLiked.GlobalVersion);
        }
    }
}