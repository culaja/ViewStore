using System.Threading.Tasks;
using Abstractions;
using MongoDB.Driver;

namespace Stores.MongoDb
{
    public delegate string MongoCollectionNameSupplier(string viewName);
    
    public sealed class MongoDbViewStore : IViewStore 
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly MongoCollectionNameSupplier _mongoCollectionNameSupplier;

        public MongoDbViewStore(
            IMongoDatabase mongoDatabase,
            MongoCollectionNameSupplier mongoCollectionNameSupplier)
        {
            _mongoDatabase = mongoDatabase;
            _mongoCollectionNameSupplier = mongoCollectionNameSupplier;
        }

        private IMongoCollection<T> CollectionFor<T>(string typeName) where T : IView
            => _mongoDatabase.GetCollection<T>(_mongoCollectionNameSupplier(typeName));

        public T? Read<T>(IViewId viewId) where T : IView =>
            (T?) CollectionFor<T>(typeof(T).Name)
                .Find(Builders<T>.Filter.Eq("_id", viewId.ToString()))
                .FirstOrDefault();

        public async Task<T?> ReadAsync<T>(IViewId viewId) where T : IView =>
            (T?)await CollectionFor<T>(typeof(T).Name)
                .Find(Builders<T>.Filter.Eq("_id", viewId.ToString()))
                .FirstOrDefaultAsync();

        public void Save<T>(IViewId viewId, T view) where T : IView
        {
            var result = CollectionFor<T>(view.GetType().Name).ReplaceOne(
                Builders<T>.Filter.Eq("_id", viewId.ToString()),
                view,
                new ReplaceOptions { IsUpsert = true });
            
            if (result.MatchedCount > 1)
            {
                throw new MongoDbWrongMatchedCountException(viewId, result.MatchedCount);
            }
        }

        public async Task SaveAsync<T>(IViewId viewId, T view) where T : IView
        {
            var result = await CollectionFor<T>(view.GetType().Name).ReplaceOneAsync(
                Builders<T>.Filter.Eq("_id", viewId.ToString()),
                view,
                new ReplaceOptions { IsUpsert = true });
            
            if (result.MatchedCount > 1)
            {
                throw new MongoDbWrongMatchedCountException(viewId, result.MatchedCount);
            }
        }
    }
}