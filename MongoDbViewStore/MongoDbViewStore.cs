using System.Linq;
using System.Threading.Tasks;
using Abstractions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Stores.MongoDb
{
    public delegate string MongoCollectionNameSupplier(string viewName);

    public sealed class MongoDbViewStore<T> : IViewStore<T> where T : IView 
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

        private IMongoCollection<T> Collection()
            => _mongoDatabase.GetCollection<T>(_mongoCollectionNameSupplier(typeof(T).Name));

        public long? ReadGlobalVersion() =>
            Collection()
                .AsQueryable()
                .OrderByDescending(t => t.GlobalVersion)
                .FirstOrDefault()?.GlobalVersion;

        public async Task<long?> ReadGlobalVersionAsync()
        {
            var lastT = await Collection()
                .AsQueryable()
                .OrderByDescending(t => t.GlobalVersion)
                .FirstOrDefaultAsync();
            return lastT?.GlobalVersion;
        }

        public T? Read(string viewId) =>
            (T?) Collection()
                .Find(Builders<T>.Filter.Eq("_id", viewId))
                .FirstOrDefault();

        public async Task<T?> ReadAsync(string viewId) =>
            (T?)await Collection()
                .Find(Builders<T>.Filter.Eq("_id", viewId))
                .FirstOrDefaultAsync();

        public void Save(T view)
        {
            var result = Collection().ReplaceOne(
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
            var result = await Collection().ReplaceOneAsync(
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