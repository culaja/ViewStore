using ViewStore.Abstractions;
using Xunit;

namespace ViewStore.MongoDb
{
    [Collection("Database collection")]
    public sealed class MongoDbViewStoreTests : ViewStoreTests
    {
        private readonly DatabaseFixture _databaseFixture;

        public MongoDbViewStoreTests(DatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
        }

        protected override IViewStore BuildViewStore() => _databaseFixture.BuildMongoDbViewStore();
    }
}