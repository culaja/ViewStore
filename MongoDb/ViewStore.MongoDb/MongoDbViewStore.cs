using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using ViewStore.Abstractions;
using static MongoDB.Driver.Builders<ViewStore.Abstractions.ViewEnvelope>;
using static MongoDB.Driver.FilterDefinition<ViewStore.Abstractions.ViewEnvelope>;

namespace ViewStore.MongoDb
{
    internal sealed class MongoDbViewStore : IViewStore
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly string _collectionName;

        private IMongoCollection<ViewEnvelope> Collection() =>
            _mongoDatabase.GetCollection<ViewEnvelope>(_collectionName);

        static MongoDbViewStore()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(ViewEnvelope)))
            {
                BsonClassMap.RegisterClassMap<ViewEnvelope>(cm =>
                {
                    cm.MapProperty(m => m.Id);
                    cm.MapProperty(m => m.View);
                    cm.MapProperty(m => m.GlobalVersion).SetSerializer(new GlobalVersionSerializer());
                    cm.MapProperty(m => m.MetaData);
                    cm.MapCreator(m => new ViewEnvelope(m.Id, m.View, m.GlobalVersion, m.MetaData));
                });    
            }
        }

        public MongoDbViewStore(IMongoDatabase mongoDatabase, string collectionName)
        {
            _mongoDatabase = mongoDatabase;
            _collectionName = collectionName;
        }

        public GlobalVersion? ReadLastGlobalVersion() =>
            Collection()
                .Find(Empty)
                .SortByDescending(view => view.GlobalVersion)
                .Limit(1)
                .FirstOrDefault()
                ?.GlobalVersion;

        public async Task<GlobalVersion?> ReadLastGlobalVersionAsync()
        {
            var lastUpdatedView = await Collection()
                .Find(Empty)
                .SortByDescending(view => view.GlobalVersion)
                .Limit(1)
                .FirstOrDefaultAsync();
            return lastUpdatedView?.GlobalVersion;
        }

        public ViewEnvelope? Read(string viewId) =>
            Collection()
                .Find(Filter.Eq("_id", viewId))
                .FirstOrDefault();

        public async Task<ViewEnvelope?> ReadAsync(string viewId) =>
            await Collection()
                .Find(Filter.Eq("_id", viewId))
                .FirstOrDefaultAsync();

        public void Save(ViewEnvelope viewEnvelope)
        {
            var result = Collection().ReplaceOne(
                Filter.Eq("_id", viewEnvelope.Id),
                viewEnvelope,
                new ReplaceOptions { IsUpsert = true });
            
            if (result.MatchedCount > 1)
            {
                throw new MongoDbWrongMatchedCountException(viewEnvelope.Id, result.MatchedCount);
            }
        }

        public async Task SaveAsync(ViewEnvelope viewEnvelope)
        {
            var result = await Collection().ReplaceOneAsync(
                Filter.Eq("_id", viewEnvelope.Id),
                viewEnvelope,
                new ReplaceOptions { IsUpsert = true });
            
            if (result.MatchedCount > 1)
            {
                throw new MongoDbWrongMatchedCountException(viewEnvelope.Id, result.MatchedCount);
            }
        }

        public void Save(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            foreach (var viewEnvelope in viewEnvelopes)
            {
                Save(viewEnvelope);
            }
        }

        public Task SaveAsync(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            return Task.WhenAll(viewEnvelopes.Select(SaveAsync));
        }

        public void Delete(ViewEnvelope viewEnvelope)
        {
            Collection().DeleteOne(Filter.Eq("_id", viewEnvelope.Id));
        }

        public async Task DeleteAsync(ViewEnvelope viewEnvelope)
        {
            await Collection().DeleteOneAsync(Filter.Eq("_id", viewEnvelope.Id));
        }
    }
}