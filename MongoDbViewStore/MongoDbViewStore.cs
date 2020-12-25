using System.Threading.Tasks;
using Abstractions;
using MongoDB.Driver;

namespace Stores.MongoDb
{
    public sealed class MongoDbViewStore<T> : IViewStore<T> where T : IView 
    {
        private readonly IMongoCollection<T> _collection;

        public MongoDbViewStore(IMongoCollection<T> collection)
        {
            _collection = collection;
        }
        
        public T? Read(string viewId) =>
            (T?) _collection
                .Find(Builders<T>.Filter.Eq("_id", viewId))
                .FirstOrDefault();

        public async Task<T?> ReadAsync(string viewId) =>
            (T?)await _collection
                .Find(Builders<T>.Filter.Eq("_id", viewId))
                .FirstOrDefaultAsync();

        public void Save(T view)
        {
            var result = _collection.ReplaceOne(
                Builders<T>.Filter.Eq("_id", view.Id),
                view,
                new ReplaceOptions { IsUpsert = true });
            
            if (result.MatchedCount > 1)
            {
                throw new MongoDbWrongMatchedCountException(view.Id, result.MatchedCount);
            }
        }

        public async Task SaveAsync(T view)
        {
            var result = await _collection.ReplaceOneAsync(
                Builders<T>.Filter.Eq("_id", view.Id),
                view,
                new ReplaceOptions { IsUpsert = true });
            
            if (result.MatchedCount > 1)
            {
                throw new MongoDbWrongMatchedCountException(view.Id, result.MatchedCount);
            }
        }
    }
}