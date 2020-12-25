using System.Threading.Tasks;
using Abstractions;
using MongoDB.Driver;
using static Stores.MongoDb.ViewMetaData;

namespace Stores.MongoDb
{
    public sealed class MongoViewStorePositionTracker : IViewStorePositionTracker
    {
        private readonly IMongoCollection<ViewMetaData> _collection;

        public MongoViewStorePositionTracker(IMongoDatabase mongoDatabase, string collectionName)
        {
            _collection = mongoDatabase.GetCollection<ViewMetaData>(collectionName);
        }
        
        public long? ReadLastGlobalVersion() =>
            _collection
                .Find(Builders<ViewMetaData>.Filter.Eq("_id", VIewMetaDataId))
                .FirstOrDefault()?.LastKnownGlobalVersion;

        public async Task<long?> ReadLastGlobalVersionAsync()
        {
            var resultCollection = await _collection
                .FindAsync(Builders<ViewMetaData>.Filter.Eq("_id", VIewMetaDataId));
            return resultCollection.FirstOrDefault()?.LastKnownGlobalVersion;
        }

        public void StoreLastGlobalVersion(long globalPosition)
        {
            var result = _collection.ReplaceOne(
                Builders<ViewMetaData>.Filter.Eq("_id", VIewMetaDataId),
                ViewMetaDataFrom(globalPosition),
                new ReplaceOptions { IsUpsert = true });
            
            if (result.MatchedCount > 1)
            {
                throw new MongoDbWrongMatchedCountException(VIewMetaDataId, result.MatchedCount);
            }
        }

        public async Task StoreLastGlobalVersionFromAsync(long globalPosition)
        {
            var result = await _collection.ReplaceOneAsync(
                Builders<ViewMetaData>.Filter.Eq("_id", VIewMetaDataId),
                ViewMetaDataFrom(globalPosition),
                new ReplaceOptions { IsUpsert = true });
            
            if (result.MatchedCount > 1)
            {
                throw new MongoDbWrongMatchedCountException(VIewMetaDataId, result.MatchedCount);
            }
        }
    }
}