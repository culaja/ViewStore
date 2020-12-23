using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Abstractions;
using Cache;
using FluentAssertions;
using Xunit;
using static Tests.TestView;
using static Tests.TestViewId;

namespace Tests
{
    public sealed class StoreCacheTests
    {
        private readonly InMemoryStore _realStore = new();
        private readonly ConcurrentDictionary<IViewId, IView> _outgoingCache = new();
        private readonly ManualCacheDrainer _manualCacheDrainer;
        private readonly ViewStoreCache _viewStoreCache;

        public StoreCacheTests()
        {
            _manualCacheDrainer = new ManualCacheDrainer(_realStore, _outgoingCache, 10);
            _viewStoreCache = new ViewStoreCache(_realStore, _outgoingCache, TimeSpan.Zero);
        }
        
        [Fact]
        public async Task saved_view_is_in_real_store_after_drain()
        {
            await _viewStoreCache.SaveAsync(TestViewId1, TestView1);
            _manualCacheDrainer.DrainCache();
            var actualView = await _realStore.ReadAsync<TestView>(TestViewId1);
            actualView.Should().Be(TestView1);
        }
        
        [Fact]
        public async Task saved_view_is_not_in_real_store_before_drain()
        {
            await _viewStoreCache.SaveAsync(TestViewId1, TestView1);
            var actualView = await _realStore.ReadAsync<TestView>(TestViewId1);
            actualView.Should().BeNull();
        }
        
        [Fact]
        public async Task saved_view_can_be_read_from_cached_store_before_drain()
        {
            await _viewStoreCache.SaveAsync(TestViewId1, TestView1);
            var actualView = await _viewStoreCache.ReadAsync<TestView>(TestViewId1);
            actualView.Should().Be(TestView1);
        }

        [Fact]
        public async Task view_can_be_read_from_real_store_before_saved_to_cache()
        {
            await _realStore.SaveAsync(TestViewId1, TestView1);
            var actualView = await _viewStoreCache.ReadAsync<TestView>(TestViewId1);
            actualView.Should().Be(TestView1);
        }
        
        [Fact]
        public async Task no_view_can_be_read_if_both_real_and_cached_store_are_empty()
        {
            var actualView = await _viewStoreCache.ReadAsync<TestView>(TestViewId1);
            actualView.Should().BeNull();
        }
    }
}