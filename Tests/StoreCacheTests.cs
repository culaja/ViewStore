using System.Threading.Tasks;
using FluentAssertions;
using ViewStore.Cache;
using Xunit;
using static ViewStore.Abstractions.TestView;

namespace Tests
{
    public sealed class StoreCacheTests
    {
        private readonly InMemoryDatabaseProvider _databaseProvider = new();
        private readonly OutgoingCache _outgoingCache = new(1000, null);
        private readonly ManualCacheDrainer _manualCacheDrainer;
        private readonly ViewStoreCacheInternal _viewStoreCacheInternal;

        public StoreCacheTests()
        {
            _manualCacheDrainer = new ManualCacheDrainer(_databaseProvider, _outgoingCache, 10);
            _viewStoreCacheInternal = new ViewStoreCacheInternal(_databaseProvider, _outgoingCache);
        }
        
        [Fact]
        public async Task saved_view_is_in_database_after_drain()
        {
            _viewStoreCacheInternal.Save(TestViewEnvelope1);
            _manualCacheDrainer.DrainCacheUntilEmpty();
            var actualView = await _databaseProvider.ReadAsync(TestViewEnvelope1.Id);
            actualView.Should().Be(TestViewEnvelope1);
        }
        
        [Fact]
        public async Task saved_view_is_not_in_database_before_drain()
        {
            _viewStoreCacheInternal.Save(TestViewEnvelope1);
            var actualView = await _databaseProvider.ReadAsync(TestViewEnvelope1.Id);
            actualView.Should().BeNull();
        }
        
        [Fact]
        public async Task saved_view_can_be_read_from_cached_store_before_drain()
        {
            _viewStoreCacheInternal.Save(TestViewEnvelope1);
            var actualView = await _viewStoreCacheInternal.Read(TestViewEnvelope1.Id);
            actualView.Should().Be(TestViewEnvelope1);
        }

        [Fact]
        public async Task view_can_be_read_from_database_before_saved_to_cache()
        {
            await _databaseProvider.UpsertAsync(new [] { TestViewEnvelope1 });
            var actualView = await _viewStoreCacheInternal.Read(TestViewEnvelope1.Id);
            actualView.Should().Be(TestViewEnvelope1);
        }
        
        [Fact]
        public async Task no_view_can_be_read_if_both_database_and_cache_are_empty()
        {
            var actualView = await _viewStoreCacheInternal.Read(TestViewEnvelope1.Id);
            actualView.Should().BeNull();
        } 
    }
}