using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using ViewStore.Abstractions;
using static MongoDB.Driver.FilterDefinition<ViewStore.MongoDb.View>;

namespace ViewStore.MongoDb
{
    public sealed class MongoDbViewStore : IViewStore
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly string _collectionName;

        private IMongoCollection<T> Collection<T>() where T : IView =>
            _mongoDatabase.GetCollection<T>(_collectionName);

        public MongoDbViewStore(IMongoDatabase mongoDatabase, string collectionName)
        {
            _mongoDatabase = mongoDatabase;
            _collectionName = collectionName;
        }

        public long? ReadLastKnownPosition() =>
            Collection<View>()
                .Find(Empty)
                .SortByDescending(view => view.GlobalVersion)
                .Limit(1)
                .FirstOrDefault()
                ?.GlobalVersion;

        public async Task<long?> ReadLastKnownPositionAsync()
        {
            var lastUpdatedView = await Collection<View>()
                .Find(Empty)
                .SortByDescending(view => view.GlobalVersion)
                .Limit(1)
                .FirstOrDefaultAsync();
            return lastUpdatedView?.GlobalVersion;
        }

        public T? Read<T>(string viewId) where T : IView =>
            (T?) Collection<T>()
                .Find(Builders<T>.Filter.Eq("_id", viewId))
                .FirstOrDefault();

        public async Task<T?> ReadAsync<T>(string viewId) where T : IView =>
            (T?)await Collection<T>()
                .Find(Builders<T>.Filter.Eq("_id", viewId))
                .FirstOrDefaultAsync();

        public void Save(IView view)
        {
            var result = Collection<IView>().ReplaceOne(
                Builders<IView>.Filter.Eq("_id", view.Id),
                view,
                new ReplaceOptions { IsUpsert = true });
            
            if (result.MatchedCount > 1)
            {
                throw new MongoDbWrongMatchedCountException(view.Id, result.MatchedCount);
            }
        }

        public async Task SaveAsync(IView view)
        {
            var result = await Collection<IView>().ReplaceOneAsync(
                Builders<IView>.Filter.Eq("_id", view.Id),
                view,
                new ReplaceOptions { IsUpsert = true });
            
            if (result.MatchedCount > 1)
            {
                throw new MongoDbWrongMatchedCountException(view.Id, result.MatchedCount);
            }
        }
    }
}