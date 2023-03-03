using ViewStore.Abstractions;
using Xunit;

namespace ViewStore.MongoDb
{
    [Collection("Database collection")]
    public sealed class MongoDbViewStoreAsyncTests : ViewStoreAsyncTests
    {
        private readonly DatabaseFixture _databaseFixture;

        public MongoDbViewStoreAsyncTests(DatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
        }

        protected override IViewStore BuildViewStore() => _databaseFixture.BuildMongoDbViewStore();
    }
}