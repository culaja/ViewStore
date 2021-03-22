using System;
using Mongo2Go;
using MongoDB.Driver;
using ViewStore.MongoDb;
using Xunit;

namespace MongoDbTests
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
        
        public MongoDbViewStore BuildMongoDbViewStore() =>
            new(
                new MongoClient(_mongoDbRunner.ConnectionString).GetDatabase(Guid.NewGuid().ToString()),
                Guid.NewGuid().ToString());
        
        public void Dispose()
        {
            _mongoDbRunner.Dispose();
        }
    }
}