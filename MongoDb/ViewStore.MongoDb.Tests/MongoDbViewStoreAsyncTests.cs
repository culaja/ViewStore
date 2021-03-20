using System;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Driver;
using ViewStore.Abstractions;
using ViewStore.MongoDb;
using ViewStore.Tests;
using Xunit;
using static ViewStore.Tests.TestView;

namespace MongoDbTests
{
    public sealed class MongoDbViewStoreAsyncTests
    {
        private MongoDbViewStore BuildEmptyMongoDbViewStore() =>
            new(
                new MongoClient("mongodb://localhost:27017").GetDatabase(Guid.NewGuid().ToString()),
                Guid.NewGuid().ToString());
        
        [Fact]
        public async Task when_view_store_is_empty_last_known_position_is_null()
        {
            var viewStore = BuildEmptyMongoDbViewStore();

            (await viewStore.ReadLastKnownPositionAsync()).Should().BeNull();
        }

        [Fact]
        public async Task after_saving_single_view_last_known_position_point_to_views_position()
        {
            var viewStore = BuildEmptyMongoDbViewStore();
            
            await viewStore.SaveAsync(TestViewEnvelope1);

            (await viewStore.ReadLastKnownPositionAsync()).Should().Be(TestViewEnvelope1.GlobalVersion);
        }
        
        [Fact]
        public async Task after_saving_multiple_views_last_known_position_point_to__greatest_view_position()
        {
            var viewStore = BuildEmptyMongoDbViewStore();
            
            await viewStore.SaveAsync(TestViewEnvelope1);
            await viewStore.SaveAsync(TestViewEnvelope2);

            (await viewStore.ReadLastKnownPositionAsync()).Should().Be(TestViewEnvelope2.GlobalVersion);
        }

        [Fact]
        public async Task saved_view_is_correctly_retrieved_from_store()
        {
            var viewStore = BuildEmptyMongoDbViewStore();
            
            await viewStore.SaveAsync(TestViewEnvelope1);

            (await viewStore.ReadAsync(TestViewEnvelope1.Id)).Should().Be(TestViewEnvelope1);
        }
        
        [Fact]
        public async Task if_another_view_is_saved_read_returns_null()
        {
            var viewStore = BuildEmptyMongoDbViewStore();
            
            await viewStore.SaveAsync(TestViewEnvelope2);

            (await viewStore.ReadAsync(TestViewEnvelope1.Id)).Should().BeNull();
        }
        
        [Fact]
        public async Task saving_two_views_is_working_properly()
        {
            var viewStore = BuildEmptyMongoDbViewStore();
            
            await viewStore.SaveAsync(TestViewEnvelope1);
            await viewStore.SaveAsync(TestViewEnvelope2);

            (await viewStore.ReadAsync(TestViewEnvelope1.Id)).Should().Be(TestViewEnvelope1);
            (await viewStore.ReadAsync(TestViewEnvelope2.Id)).Should().Be(TestViewEnvelope2);
        }
        
        [Fact]
        public async Task saving_view_transformation_will_keep_only_last_version()
        {
            var viewStore = BuildEmptyMongoDbViewStore();

            var transformedViewEnvelope = TestViewEnvelope1.ImmutableTransform<TestView>(
                GlobalVersion.Of(1, 0),
                testView => testView.Increment());
            
            await viewStore.SaveAsync(TestViewEnvelope1);
            await viewStore.SaveAsync(transformedViewEnvelope);

            (await viewStore.ReadAsync(TestViewEnvelope1.Id)).Should().Be(transformedViewEnvelope);
        }
    }
}