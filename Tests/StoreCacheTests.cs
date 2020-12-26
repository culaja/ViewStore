using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using FluentAssertions;
using ViewStore.Cache;
using Xunit;
using static ViewStore.Tests.TestView;

namespace ViewStore.Tests
{
    public sealed class StoreCacheTests
    {
        private readonly InMemoryStore _realStore = new();
        private readonly OutgoingCache _outgoingCache = new();
        private readonly ManualCacheDrainer _manualCacheDrainer;
        private readonly ViewStoreCacheInternal _viewStoreCacheInternal;

        public StoreCacheTests()
        {
            _manualCacheDrainer = new ManualCacheDrainer(_realStore, _outgoingCache, 10);
            _viewStoreCacheInternal = new ViewStoreCacheInternal(
                _realStore,
                new MemoryCache(Guid.NewGuid().ToString()),
                _outgoingCache,
                TimeSpan.Zero);
        }
        
        [Fact]
        public async Task saved_view_is_in_real_store_after_drain()
        {
            await _viewStoreCacheInternal.SaveAsync(TestView1);
            _manualCacheDrainer.TryDrainCache();
            var actualView = await _realStore.ReadAsync<TestView>(TestView1.Id);
            actualView.Should().Be(TestView1);
        }
        
        [Fact]
        public async Task saved_view_is_not_in_real_store_before_drain()
        {
            await _viewStoreCacheInternal.SaveAsync(TestView1);
            var actualView = await _realStore.ReadAsync<TestView>(TestView1.Id);
            actualView.Should().BeNull();
        }
        
        [Fact]
        public async Task saved_view_can_be_read_from_cached_store_before_drain()
        {
            await _viewStoreCacheInternal.SaveAsync(TestView1);
            var actualView = await _viewStoreCacheInternal.ReadAsync<TestView>(TestView1.Id);
            actualView.Should().Be(TestView1);
        }

        [Fact]
        public async Task view_can_be_read_from_real_store_before_saved_to_cache()
        {
            await _realStore.SaveAsync(TestView1);
            var actualView = await _viewStoreCacheInternal.ReadAsync<TestView>(TestView1.Id);
            actualView.Should().Be(TestView1);
        }
        
        [Fact]
        public async Task no_view_can_be_read_if_both_real_and_cached_store_are_empty()
        {
            var actualView = await _viewStoreCacheInternal.ReadAsync<TestView>(TestView1.Id);
            actualView.Should().BeNull();
        }
    }
}