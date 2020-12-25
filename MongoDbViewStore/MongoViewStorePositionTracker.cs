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
        
        public long? ReadLastKnownPosition() =>
            _collection
                .Find(Builders<ViewMetaData>.Filter.Eq("_id", VIewMetaDataId))
                .FirstOrDefault()?.LastKnownPosition;

        public async Task<long?> ReadLastKnownPositionAsync()
        {
            var resultCollection = await _collection
                .FindAsync(Builders<ViewMetaData>.Filter.Eq("_id", VIewMetaDataId));
            return resultCollection.FirstOrDefault()?.LastKnownPosition;
        }

        public void StoreLastKnownPosition(long position)
        {
            var result = _collection.ReplaceOne(
                Builders<ViewMetaData>.Filter.Eq("_id", VIewMetaDataId),
                ViewMetaDataFrom(position),
                new ReplaceOptions { IsUpsert = true });
            
            if (result.MatchedCount > 1)
            {
                throw new MongoDbWrongMatchedCountException(VIewMetaDataId, result.MatchedCount);
            }
        }

        public async Task StoreLastKnownPositionAsync(long position)
        {
            var result = await _collection.ReplaceOneAsync(
                Builders<ViewMetaData>.Filter.Eq("_id", VIewMetaDataId),
                ViewMetaDataFrom(position),
                new ReplaceOptions { IsUpsert = true });
            
            if (result.MatchedCount > 1)
            {
                throw new MongoDbWrongMatchedCountException(VIewMetaDataId, result.MatchedCount);
            }
        }
    }
}