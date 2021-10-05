﻿using FluentAssertions;
using ViewStore.Abstractions;
using ViewStore.InMemory;
using Xunit;
using static ViewStore.Abstractions.TestView;

namespace ViewStore.Cache
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
        
        /// <remarks>
        /// ----- Current cache -----*----- Drained cache -----*----- Real Store -----
        ///           V1, V2
        /// -------------------------*-------------------------*---------------------- 
        /// </remarks>
        [Theory]
        [InlineData(2, 1,    1, 2,    2, 1)]
        [InlineData(2, 1,    2, 2,    2, 2)]
        [InlineData(0, 1,    0, 5,    0, 5)]
        public void check_last_global_version_after_adding_two_views_and_when_drain_is_not_triggered(
            long partA1, long partA2,
            long partB1, long partB2,
            long expectedPart1, long expectedPart2)
        {
            _viewStoreCacheInternal.Save(TestViewEnvelope1.WithGlobalVersion(GlobalVersion.Of(partA1, partA2)));
            _viewStoreCacheInternal.Save(TestViewEnvelope2.WithGlobalVersion(GlobalVersion.Of(partB1, partB2)));

            _viewStoreCacheInternal.ReadLastGlobalVersion()
                .Should()
                .Be(GlobalVersion.Of(expectedPart1, expectedPart2));
        }
        
        
        /// <remarks>
        /// ----- Current cache -----*----- Drained cache -----*----- Real Store -----
        ///                                    V1 V2                    V1 V2
        /// -------------------------*-------------------------*---------------------- 
        /// </remarks>
        [Theory]
        [InlineData(2, 1,    1, 2,    2, 1)]
        [InlineData(2, 1,    2, 2,    2, 2)]
        [InlineData(0, 1,    0, 5,    0, 5)]
        public void check_last_global_version_after_adding_two_views_and_when_drain_is_triggered(
            long partA1, long partA2,
            long partB1, long partB2,
            long expectedPart1, long expectedPart2)
        {
            _viewStoreCacheInternal.Save(TestViewEnvelope1.WithGlobalVersion(GlobalVersion.Of(partA1, partA2)));
            _viewStoreCacheInternal.Save(TestViewEnvelope2.WithGlobalVersion(GlobalVersion.Of(partB1, partB2)));

            _manualCacheDrainer.DrainCacheUntilEmpty();

            _viewStoreCacheInternal.ReadLastGlobalVersion()
                .Should()
                .Be(GlobalVersion.Of(expectedPart1, expectedPart2));
        }
        
        /// <remarks>
        /// ----- Current cache -----*----- Drained cache -----*----- Real Store -----
        ///              V2                       V1                      V1
        /// -------------------------*-------------------------*---------------------- 
        /// </remarks>
        [Theory]
        [InlineData(2, 1,    1, 2,    2, 1)]
        [InlineData(2, 1,    2, 2,    2, 2)]
        [InlineData(0, 1,    0, 5,    0, 5)]
        public void check_last_global_version_after_adding_two_views_and_when_drain_is_triggered_between_two_saves(
            long partA1, long partA2,
            long partB1, long partB2,
            long expectedPart1, long expectedPart2)
        {
            _viewStoreCacheInternal.Save(TestViewEnvelope1.WithGlobalVersion(GlobalVersion.Of(partA1, partA2)));
            _manualCacheDrainer.DrainCacheUntilEmpty();
            _viewStoreCacheInternal.Save(TestViewEnvelope2.WithGlobalVersion(GlobalVersion.Of(partB1, partB2)));

            _viewStoreCacheInternal.ReadLastGlobalVersion()
                .Should()
                .Be(GlobalVersion.Of(expectedPart1, expectedPart2));
        }
        
        /// <remarks>
        /// ----- Current cache -----*----- Drained cache -----*----- Real Store -----
        ///                                                             V1, V2
        /// -------------------------*-------------------------*---------------------- 
        /// </remarks>
        [Theory]
        [InlineData(2, 1,    1, 2,    2, 1)]
        [InlineData(2, 1,    2, 2,    2, 2)]
        [InlineData(0, 1,    0, 5,    0, 5)]
        public void check_last_global_version_after_adding_two_views_and_after_two_consecutive_drains(
            long partA1, long partA2,
            long partB1, long partB2,
            long expectedPart1, long expectedPart2)
        {
            _viewStoreCacheInternal.Save(TestViewEnvelope1.WithGlobalVersion(GlobalVersion.Of(partA1, partA2)));
            _viewStoreCacheInternal.Save(TestViewEnvelope2.WithGlobalVersion(GlobalVersion.Of(partB1, partB2)));

            _manualCacheDrainer.DrainCacheUntilEmpty();
            _manualCacheDrainer.DrainCacheUntilEmpty();

            _viewStoreCacheInternal.ReadLastGlobalVersion()
                .Should()
                .Be(GlobalVersion.Of(expectedPart1, expectedPart2));
        }
        
        [Fact]
        public void check_last_global_version_after_adding_a_view_to_final_store_without_caching()
        {
            _finalStore.Save(TestViewEnvelope1.WithGlobalVersion(GlobalVersion.Of(5, 2)));

            _viewStoreCacheInternal.ReadLastGlobalVersion()
                .Should()
                .Be(GlobalVersion.Of(5, 2));
        }
    }
}