﻿using ViewStore.MongoDb;
using ViewStore.Tests;
using Xunit;

namespace MongoDbTests
{
    [Collection("Database collection")]
    public sealed class MongoDbViewStoreAsyncTests : ViewStoreAsyncTests
    {
        private readonly DatabaseFixture _databaseFixture;

        public MongoDbViewStoreAsyncTests(DatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
        }

        protected override MongoDbViewStore BuildViewStore() => _databaseFixture.BuildMongoDbViewStore();
    }
}