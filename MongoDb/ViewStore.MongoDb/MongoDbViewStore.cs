using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using ViewStore.Abstractions;
using static MongoDB.Driver.Builders<ViewStore.MongoDb.ViewEnvelopeInternal>;
using static MongoDB.Driver.FilterDefinition<ViewStore.MongoDb.ViewEnvelopeInternal>;

namespace ViewStore.MongoDb
{
    internal sealed class MongoDbViewStore : IViewStore
    {
        private const string LastDeletedViewId = "LastDeleted-b24ae98262724d27bd8e31c34ff11f1a";
        
        private readonly IMongoDatabase _mongoDatabase;
        private readonly string _collectionName;

        private IMongoCollection<ViewEnvelopeInternal> Collection() =>
            _mongoDatabase.GetCollection<ViewEnvelopeInternal>(_collectionName);

        static MongoDbViewStore()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(ViewEnvelopeInternal)))
            {
                BsonClassMap.RegisterClassMap<ViewEnvelopeInternal>(cm =>
                {
                    cm.MapProperty(m => m.Id);
                    cm.MapProperty(m => m.View);
                    cm.MapProperty(m => m.GlobalVersion);
                    cm.MapProperty(m => m.MetaData);
                    cm.MapProperty(m => m.TenantId);
                    cm.MapProperty(m => m.CreatedAt);
                    cm.MapCreator(m => new ViewEnvelopeInternal(m.Id, m.View, m.GlobalVersion, m.MetaData, m.TenantId, m.CreatedAt));
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
                .SortByDescending(vei => vei.GlobalVersion)
                .Limit(1)
                .ToList()
                .Select(vei => vei.ToViewEnvelope())
                .FirstOrDefault()
                ?.GlobalVersion;

        public async Task<GlobalVersion?> ReadLastGlobalVersionAsync()
        {
            var lastUpdatedView = await Collection()
                .Find(Empty)
                .SortByDescending(view => view.GlobalVersion)
                .Limit(1)
                .FirstOrDefaultAsync();
            return lastUpdatedView != null ? GlobalVersion.Of(lastUpdatedView.GlobalVersion) : null;
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
                viewEnvelope.ToViewEnvelopeInternal(),
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
                viewEnvelope.ToViewEnvelopeInternal(),
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

        public Task SaveAsync(IEnumerable<ViewEnvelope> viewEnvelopes) => Task.WhenAll(viewEnvelopes.Select(SaveAsync));

        public void Delete(string viewId, GlobalVersion globalVersion)
        {
            Collection().DeleteOne(Filter.Eq("_id", viewId));
            Save(ViewEnvelope.EmptyWith(LastDeletedViewId, globalVersion));
        }

        public async Task DeleteAsync(string viewId, GlobalVersion globalVersion)
        {
            await Collection().DeleteOneAsync(Filter.Eq("_id", viewId));
            await SaveAsync(ViewEnvelope.EmptyWith(LastDeletedViewId, globalVersion));
        }

        public void Delete(IEnumerable<string> viewIds, GlobalVersion globalVersion)
        {
            Collection().DeleteMany(Filter.In("_id", viewIds.Select(viewId => viewId)));
            Save(ViewEnvelope.EmptyWith(LastDeletedViewId, globalVersion));
        }

        public async Task DeleteAsync(IEnumerable<string> viewIds, GlobalVersion globalVersion)
        {
            await Collection().DeleteManyAsync(Filter.In("_id", viewIds.Select(viewId => viewId)));
            await SaveAsync(ViewEnvelope.EmptyWith(LastDeletedViewId, globalVersion));
        }
    }
}