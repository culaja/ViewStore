using System;
using Mongo2Go;
using MongoDB.Driver;
using ViewStore.Abstractions;
using Xunit;

namespace ViewStore.MongoDb
{
    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
    
    public sealed class DatabaseFixture : IDisposable
    {
        private readonly MongoDbRunner _mongoDbRunner;

        public DatabaseFixture()
        {
            _mongoDbRunner = MongoDbRunner.Start();
        }

        public IViewStore BuildMongoDbViewStore() =>
            MongoDbViewStoreBuilder.New()
                .WithConnectionDetails(_mongoDbRunner.ConnectionString, Guid.NewGuid().ToString())
                .WithCollectionName(Guid.NewGuid().ToString())
                .Build();
        
        public void Dispose()
        {
            _mongoDbRunner.Dispose();
        }
    }
}