using System.Threading.Tasks;
using FluentAssertions;
using ViewStore.Abstractions;
using ViewStore.InMemory;
using Xunit;
using static ViewStore.Abstractions.TestView;

namespace ViewStore.WriteBehindCache
{
    public sealed class FetchingLastGlobalVersionFromCacheTests
    {
        private readonly InMemoryViewStore _finalStore = new();
        private readonly OutgoingCache _outgoingCache = new();
        private readonly ManualCacheDrainer _manualCacheDrainer;
        private readonly ViewStoreCacheInternal _viewStoreCacheInternal;

        public FetchingLastGlobalVersionFromCacheTests()
        {
            _manualCacheDrainer = new ManualCacheDrainer(_finalStore, _outgoingCache, 10);
            _viewStoreCacheInternal = new ViewStoreCacheInternal(_finalStore, _outgoingCache);
        }
        
        [Theory]
        [InlineData(2, 1,    1, 2,    2, 1)]
        [InlineData(2, 1,    2, 2,    2, 2)]
        [InlineData(0, 1,    0, 5,    0, 5)]
        public async Task check_last_global_version_after_adding_two_views_and_when_drain_is_not_triggered(
            long partA1, long partA2,
            long partB1, long partB2,
            long expectedPart1, long expectedPart2)
        {
            await _viewStoreCacheInternal.SaveAsync(TestViewEnvelope1.WithGlobalVersion(GlobalVersion.Of(partA1, partA2)));
            await _viewStoreCacheInternal.SaveAsync(TestViewEnvelope2.WithGlobalVersion(GlobalVersion.Of(partB1, partB2)));

            (await _viewStoreCacheInternal.ReadLastGlobalVersionAsync())
                .Should()
                .Be(GlobalVersion.Of(expectedPart1, expectedPart2));
        }
        
        
        [Theory]
        [InlineData(2, 1,    1, 2,    2, 1)]
        [InlineData(2, 1,    2, 2,    2, 2)]
        [InlineData(0, 1,    0, 5,    0, 5)]
        public async Task check_last_global_version_after_adding_two_views_and_when_drain_is_triggered(
            long partA1, long partA2,
            long partB1, long partB2,
            long expectedPart1, long expectedPart2)
        {
            await _viewStoreCacheInternal.SaveAsync(TestViewEnvelope1.WithGlobalVersion(GlobalVersion.Of(partA1, partA2)));
            await _viewStoreCacheInternal.SaveAsync(TestViewEnvelope2.WithGlobalVersion(GlobalVersion.Of(partB1, partB2)));

            _manualCacheDrainer.DrainCacheUntilEmpty();

            (await _viewStoreCacheInternal.ReadLastGlobalVersionAsync())
                .Should()
                .Be(GlobalVersion.Of(expectedPart1, expectedPart2));
        }
        
        [Theory]
        [InlineData(2, 1,    1, 2,    2, 1)]
        [InlineData(2, 1,    2, 2,    2, 2)]
        [InlineData(0, 1,    0, 5,    0, 5)]
        public async Task check_last_global_version_after_adding_two_views_and_when_drain_is_triggered_between_two_saves(
            long partA1, long partA2,
            long partB1, long partB2,
            long expectedPart1, long expectedPart2)
        {
            await _viewStoreCacheInternal.SaveAsync(TestViewEnvelope1.WithGlobalVersion(GlobalVersion.Of(partA1, partA2)));
            _manualCacheDrainer.DrainCacheUntilEmpty();
            await _viewStoreCacheInternal.SaveAsync(TestViewEnvelope2.WithGlobalVersion(GlobalVersion.Of(partB1, partB2)));

            (await _viewStoreCacheInternal.ReadLastGlobalVersionAsync())
                .Should()
                .Be(GlobalVersion.Of(expectedPart1, expectedPart2));
        }
        
        [Theory]
        [InlineData(2, 1,    1, 2,    2, 1)]
        [InlineData(2, 1,    2, 2,    2, 2)]
        [InlineData(0, 1,    0, 5,    0, 5)]
        public async Task check_last_global_version_after_adding_two_views_and_after_two_consecutive_drains(
            long partA1, long partA2,
            long partB1, long partB2,
            long expectedPart1, long expectedPart2)
        {
            await _viewStoreCacheInternal.SaveAsync(TestViewEnvelope1.WithGlobalVersion(GlobalVersion.Of(partA1, partA2)));
            await _viewStoreCacheInternal.SaveAsync(TestViewEnvelope2.WithGlobalVersion(GlobalVersion.Of(partB1, partB2)));

            _manualCacheDrainer.DrainCacheUntilEmpty();
            _manualCacheDrainer.DrainCacheUntilEmpty();

            (await _viewStoreCacheInternal.ReadLastGlobalVersionAsync())
                .Should()
                .Be(GlobalVersion.Of(expectedPart1, expectedPart2));
        }
    }
}