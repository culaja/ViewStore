using System;
using FluentAssertions;
using MongoDB.Driver;
using ViewStore.MongoDb;
using Xunit;
using static ViewStore.Tests.TestView;

namespace ViewStore.Tests
{
    public sealed class MongoDbViewStoreTests
    {
        private MongoDbViewStore BuildEmptyMongoDbViewStore() =>
            new(
                new MongoClient("mongodb://kolotree:dagi987@localhost:27017").GetDatabase(Guid.NewGuid().ToString()),
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
            
            viewStore.Save(TestView1);

            viewStore.ReadLastKnownPosition().Should().Be(TestView1.GlobalVersion);
        }
        
        [Fact]
        public void after_saving_multiple_views_last_known_position_point_to__greatest_view_position()
        {
            var viewStore = BuildEmptyMongoDbViewStore();
            
            viewStore.Save(TestView1);
            viewStore.Save(TestView2);

            viewStore.ReadLastKnownPosition().Should().Be(TestView2.GlobalVersion);
        }

        [Fact]
        public void saved_view_is_correctly_retrieved_from_store()
        {
            var viewStore = BuildEmptyMongoDbViewStore();
            
            viewStore.Save(TestView1);

            viewStore.Read<TestView>(TestView1.Id).Should().Be(TestView1);
        }
        
        [Fact]
        public void if_another_view_is_saved_read_returns_null()
        {
            var viewStore = BuildEmptyMongoDbViewStore();
            
            viewStore.Save(TestView2);

            viewStore.Read<TestView>(TestView1.Id).Should().BeNull();
        }
        
        [Fact]
        public void saving_two_views_is_working_properly()
        {
            var viewStore = BuildEmptyMongoDbViewStore();
            
            viewStore.Save(TestView1);
            viewStore.Save(TestView2);

            viewStore.Read<TestView>(TestView1.Id).Should().Be(TestView1);
            viewStore.Read<TestView>(TestView2.Id).Should().Be(TestView2);
        }
        
        [Fact]
        public void saving_view_transformation_will_keep_only_last_version()
        {
            var viewStore = BuildEmptyMongoDbViewStore();
            
            viewStore.Save(TestView1.IncrementNumber());

            viewStore.Read<TestView>(TestView1.Id).Should().Be(TestView1.IncrementNumber());
        }
    }
}