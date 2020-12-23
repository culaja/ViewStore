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

        private IMongoCollection<T> CollectionFor<T>() where T : IView
            => _mongoDatabase.GetCollection<T>(_mongoCollectionNameSupplier(typeof(T).Name));
        
        public async Task<T?> ReadAsync<T>(IViewId viewId) where T : IView =>
            (T?)await CollectionFor<T>()
                .Find(Builders<T>.Filter.Eq("_id", viewId.ToString()))
                .FirstOrDefaultAsync();

        public async Task SaveAsync<T>(IViewId viewId, T view) where T : IView
        {
            var result = await CollectionFor<T>().ReplaceOneAsync(
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