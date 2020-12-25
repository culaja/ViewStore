using System;
using System.Threading.Tasks;
using Cache;
using FluentAssertions;
using Xunit;
using static Tests.TestView;

namespace Tests
{
    public sealed class StoreCacheTests
    {
        private readonly InMemoryStore<TestView> _realStore = new();
        private readonly OutgoingCache<TestView> _outgoingCache = new();
        private readonly ManualCacheDrainer<TestView> _manualCacheDrainer;
        private readonly ViewStoreCache<TestView> _viewStoreCache;

        public StoreCacheTests()
        {
            _manualCacheDrainer = new ManualCacheDrainer<TestView>(_realStore, _outgoingCache, 10);
            _viewStoreCache = new ViewStoreCache<TestView>(_realStore, _outgoingCache, TimeSpan.Zero);
        }
        
        [Fact]
        public async Task saved_view_is_in_real_store_after_drain()
        {
            await _viewStoreCache.SaveAsync(TestView1);
            _manualCacheDrainer.TryDrainCache();
            var actualView = await _realStore.ReadAsync(TestView1.Id);
            actualView.Should().Be(TestView1);
        }
        
        [Fact]
        public async Task saved_view_is_not_in_real_store_before_drain()
        {
            await _viewStoreCache.SaveAsync(TestView1);
            var actualView = await _realStore.ReadAsync(TestView1.Id);
            actualView.Should().BeNull();
        }
        
        [Fact]
        public async Task saved_view_can_be_read_from_cached_store_before_drain()
        {
            await _viewStoreCache.SaveAsync(TestView1);
            var actualView = await _viewStoreCache.ReadAsync(TestView1.Id);
            actualView.Should().Be(TestView1);
        }

        [Fact]
        public async Task view_can_be_read_from_real_store_before_saved_to_cache()
        {
            await _realStore.SaveAsync(TestView1);
            var actualView = await _viewStoreCache.ReadAsync(TestView1.Id);
            actualView.Should().Be(TestView1);
        }
        
        [Fact]
        public async Task no_view_can_be_read_if_both_real_and_cached_store_are_empty()
        {
            var actualView = await _viewStoreCache.ReadAsync(TestView1.Id);
            actualView.Should().BeNull();
        }
    }
}