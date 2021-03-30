using System;
using MongoDB.Driver;
using ViewStore.Abstractions;

namespace ViewStore.MongoDb
{
    public sealed class MongoDbViewStoreBuilder
    {
        private IMongoDatabase? _mongoDatabase = null;
        private string? _collectionName = null;
        private Action _viewRegistrator = () => { };

        public static MongoDbViewStoreBuilder New() => new();

        public MongoDbViewStoreBuilder WithMongoDatabase(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            return this;
        }
        
        public MongoDbViewStoreBuilder WithConnectionDetails(string connectionString, string databaseName)
        {
            _mongoDatabase = new MongoClient(connectionString).GetDatabase(databaseName);
            return this;
        }
        
        public MongoDbViewStoreBuilder WithCollectionName(string collectionName)
        {
            _collectionName = collectionName;
            return this;
        }

        public MongoDbViewStoreBuilder UseViewRegistrator(Action viewRegistrator)
        {
            _viewRegistrator = viewRegistrator;
            return this;
        }

        public IViewStore Build()
        {
            if (_mongoDatabase == null) throw new ArgumentNullException(nameof(_mongoDatabase), "Mongo database is not provided");
            if (_collectionName == null) throw new ArgumentNullException(nameof(_collectionName), "Mongo collection name is not provided");
            
            _viewRegistrator();
            
            return new MongoDbViewStore(_mongoDatabase, _collectionName);
        }
    }
}