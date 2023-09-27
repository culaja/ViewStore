using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using FluentAssertions;
using ViewStore;
using ViewStore.Abstractions;
using ViewStore.Cache;
using Xunit;
using static ViewStore.Abstractions.TestView;

namespace Tests
{
    public sealed class ViewStoreTests : IDisposable
    {
        private readonly ViewStoreCache _cache = ViewStoreCacheBuilder.New()
                .WithFlusher(new InMemoryViewStoreFlusher())
                .WithCacheDrainPeriod(TimeSpan.Zero)
                .WithReadMemoryCache(new MemoryCache(Guid.NewGuid().ToString()))
                .Build();
        
        public void Dispose()
        {
            _cache.Dispose();
        }
        
        [Fact]
        public async Task when_view_store_is_empty_last_global_version_is_null()
        {
            (await _cache.ReadLastGlobalVersion()).Should().BeNull();
        }

        [Fact]
        public async Task after_saving_single_view_last_global_version_in_store_point_to_views_global_version()
        {
            _cache.Save(TestViewEnvelope1);

            (await _cache.ReadLastGlobalVersion()).Should().Be(TestViewEnvelope1.GlobalVersion);
        }
        
        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 1, 1)]
        [InlineData(1, 2, 2)]
        [InlineData(2, 1, 2)]
        public async Task after_saving_multiple_views_last_global_version_in_store_points_to_greatest_view_global_version(
            long view1GlobalVersion,
            long view2GlobalVersion,
            long expectedLastGlobalVersion)
        {
            _cache.Save(TestViewEnvelope1.WithGlobalVersion(view1GlobalVersion));
            _cache.Save(TestViewEnvelope2.WithGlobalVersion(view2GlobalVersion));

            (await _cache.ReadLastGlobalVersion())
                .Should().Be(expectedLastGlobalVersion);
        }

        [Fact]
        public async Task saved_view_is_correctly_retrieved_from_store()
        {
            _cache.Save(TestViewEnvelope1);

            (await _cache.Read(TestViewEnvelope1.Id)).Should().Be(TestViewEnvelope1);
        }
        
        [Fact]
        public async Task if_another_view_is_saved_read_returns_null()
        {
            _cache.Save(TestViewEnvelope2);

            (await _cache.Read(TestViewEnvelope1.Id)).Should().BeNull();
        }
        
        [Fact]
        public async Task saving_two_views_is_working_properly()
        {
            _cache.Save(TestViewEnvelope1);
            _cache.Save(TestViewEnvelope2);

            (await _cache.Read(TestViewEnvelope1.Id)).Should().Be(TestViewEnvelope1);
            (await _cache.Read(TestViewEnvelope2.Id)).Should().Be(TestViewEnvelope2);
        }
        
        [Fact]
        public async Task saving_view_transformation_will_keep_only_last_version()
        {
            var transformedViewEnvelope = TestViewEnvelope1.ImmutableTransform<TestView>(
                1L,
                testView => testView.Increment());
            
            _cache.Save(TestViewEnvelope1);
            _cache.Save(transformedViewEnvelope);

            (await _cache.Read(TestViewEnvelope1.Id)).Should().Be(transformedViewEnvelope);
        }

        [Fact]
        public async Task saving_batch_of_views_works()
        {
            _cache.Save(new [] { TestViewEnvelope1, TestViewEnvelope2} );

            (await _cache.Read(TestViewEnvelope1.Id)).Should().Be(TestViewEnvelope1);
            (await _cache.Read(TestViewEnvelope2.Id)).Should().Be(TestViewEnvelope2);
        }

        [Fact]
        public async Task after_deleting_an_object_it_cant_be_found_in_store()
        {
            _cache.Save(TestViewEnvelope1);

            _cache.Delete(TestViewEnvelope1.Id, 2L);

            (await _cache.Read(TestViewEnvelope1.Id)).Should().BeNull();
        }
        
        [Fact]
        public async Task after_deleting_an_object_last_global_version_is_updated_correctly()
        {
            _cache.Save(TestViewEnvelope1);

            _cache.Delete(TestViewEnvelope1.Id, 2L);

            (await _cache.ReadLastGlobalVersion()).Should().Be(2L);
        }
        
        [Fact]
        public async Task after_deleting_batch_of_objects_those_objects_cant_be_found_in_store()
        {
            _cache.Save(TestViewEnvelope1);
            _cache.Save(TestViewEnvelope2);

            _cache.Delete(new [] { TestViewEnvelope1.Id, TestViewEnvelope2.Id }, 3L);

            (await _cache.Read(TestViewEnvelope1.Id)).Should().BeNull();
            (await _cache.Read(TestViewEnvelope2.Id)).Should().BeNull();
        }
        
        [Fact]
        public async Task after_deleting_batch_of_objects_last_global_version_is_updated_correctly()
        {
            _cache.Save(TestViewEnvelope1);
            _cache.Save(TestViewEnvelope2);

            _cache.Delete(new [] { TestViewEnvelope1.Id, TestViewEnvelope2.Id }, 3L);

            (await _cache.ReadLastGlobalVersion()).Should().Be(3L);
        }

        
    }
}