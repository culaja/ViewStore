using System;
using FluentAssertions;
using MongoDB.Driver;
using ViewStore.Abstractions;
using ViewStore.MongoDb;
using Xunit;
using static ViewStore.Tests.TestView;

namespace ViewStore.Tests
{
    public sealed class MongoDbViewStoreTests
    {
        private MongoDbViewStore BuildEmptyMongoDbViewStore() =>
            new(
                new MongoClient("mongodb://localhost:27017").GetDatabase(Guid.NewGuid().ToString()),
                Guid.NewGuid().ToString());
        
        [Fact]
        public void when_view_store_is_empty_last_known_position_is_null()
        {
            var viewStore = BuildEmptyMongoDbViewStore();

            viewStore.ReadLastKnownPosition().Should().BeNull();
        }

        [Fact]
        public void after_saving_single_view_last_known_position_point_to_views_position()
        {
            var viewStore = BuildEmptyMongoDbViewStore();
            
            viewStore.Save(TestViewEnvelope1);

            viewStore.ReadLastKnownPosition().Should().Be(TestViewEnvelope1.GlobalVersion);
        }
        
        [Fact]
        public void after_saving_multiple_views_last_known_position_points_to_greatest_view_position()
        {
            var viewStore = BuildEmptyMongoDbViewStore();
            
            viewStore.Save(TestViewEnvelope1);
            viewStore.Save(TestViewEnvelope2);

            viewStore.ReadLastKnownPosition().Should().Be(TestViewEnvelope2.GlobalVersion);
        }

        [Fact]
        public void saved_view_is_correctly_retrieved_from_store()
        {
            var viewStore = BuildEmptyMongoDbViewStore();
            
            viewStore.Save(TestViewEnvelope1);

            viewStore.Read(TestViewEnvelope1.Id).Should().Be(TestViewEnvelope1);
        }
        
        [Fact]
        public void if_another_view_is_saved_read_returns_null()
        {
            var viewStore = BuildEmptyMongoDbViewStore();
            
            viewStore.Save(TestViewEnvelope2);

            viewStore.Read(TestViewEnvelope1.Id).Should().BeNull();
        }
        
        [Fact]
        public void saving_two_views_is_working_properly()
        {
            var viewStore = BuildEmptyMongoDbViewStore();
            
            viewStore.Save(TestViewEnvelope1);
            viewStore.Save(TestViewEnvelope2);

            viewStore.Read(TestViewEnvelope1.Id).Should().Be(TestViewEnvelope1);
            viewStore.Read(TestViewEnvelope2.Id).Should().Be(TestViewEnvelope2);
        }
        
        [Fact]
        public void saving_view_transformation_will_keep_only_last_version()
        {
            var viewStore = BuildEmptyMongoDbViewStore();

            var transformedViewEnvelope = TestViewEnvelope1.ImmutableTransform<TestView>(
                GlobalVersion.Of(1, 0),
                testView => testView.Increment());
            
            viewStore.Save(TestViewEnvelope1);
            viewStore.Save(transformedViewEnvelope);

            viewStore.Read(TestViewEnvelope1.Id).Should().Be(transformedViewEnvelope);
        }
    }
}