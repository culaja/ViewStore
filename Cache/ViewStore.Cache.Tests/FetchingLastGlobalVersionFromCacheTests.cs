using FluentAssertions;
using ViewStore.Abstractions;
using ViewStore.InMemory;
using Xunit;
using static ViewStore.Abstractions.TestView;

namespace ViewStore.Cache
{
    public sealed class FetchingLastGlobalVersionFromCacheTests
    {
        private readonly InMemoryViewStore _finalStore = new();
        private readonly OutgoingCache _outgoingCache = new(1000, null);
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
        [InlineData(2, 1, 2)]
        [InlineData(1,5, 5)]
        public void check_last_global_version_after_adding_two_views_and_when_drain_is_not_triggered(
            long view1Position,
            long view2Position,
            long expectedPosition)
        {
            _viewStoreCacheInternal.Save(TestViewEnvelope1.WithGlobalVersion(GlobalVersion.Of(view1Position)));
            _viewStoreCacheInternal.Save(TestViewEnvelope2.WithGlobalVersion(GlobalVersion.Of(view2Position)));

            _viewStoreCacheInternal.ReadLastGlobalVersion()
                .Should()
                .Be(GlobalVersion.Of(expectedPosition));
        }
        
        
        /// <remarks>
        /// ----- Current cache -----*----- Drained cache -----*----- Real Store -----
        ///                                    V1 V2                    V1 V2
        /// -------------------------*-------------------------*---------------------- 
        /// </remarks>
        [Theory]
        [InlineData(2, 1, 2)]
        [InlineData(1,5, 5)]
        public void check_last_global_version_after_adding_two_views_and_when_drain_is_triggered(
            long view1Position,
            long view2Position,
            long expectedPosition)
        {
            _viewStoreCacheInternal.Save(TestViewEnvelope1.WithGlobalVersion(GlobalVersion.Of(view1Position)));
            _viewStoreCacheInternal.Save(TestViewEnvelope2.WithGlobalVersion(GlobalVersion.Of(view2Position)));

            _manualCacheDrainer.DrainCacheUntilEmpty();

            _viewStoreCacheInternal.ReadLastGlobalVersion()
                .Should()
                .Be(GlobalVersion.Of(expectedPosition));
        }
        
        /// <remarks>
        /// ----- Current cache -----*----- Drained cache -----*----- Real Store -----
        ///              V2                       V1                      V1
        /// -------------------------*-------------------------*---------------------- 
        /// </remarks>
        [Theory]
        [InlineData(2, 1, 2)]
        [InlineData(1,5, 5)]
        public void check_last_global_version_after_adding_two_views_and_when_drain_is_triggered_between_two_saves(
            long view1Position,
            long view2Position,
            long expectedPosition)
        {
            _viewStoreCacheInternal.Save(TestViewEnvelope1.WithGlobalVersion(GlobalVersion.Of(view1Position)));
            _manualCacheDrainer.DrainCacheUntilEmpty();
            _viewStoreCacheInternal.Save(TestViewEnvelope2.WithGlobalVersion(GlobalVersion.Of(view2Position)));

            _viewStoreCacheInternal.ReadLastGlobalVersion()
                .Should()
                .Be(GlobalVersion.Of(expectedPosition));
        }
        
        /// <remarks>
        /// ----- Current cache -----*----- Drained cache -----*----- Real Store -----
        ///                                                             V1, V2
        /// -------------------------*-------------------------*---------------------- 
        /// </remarks>
        [Theory]
        [InlineData(2, 1, 2)]
        [InlineData(1,5, 5)]
        public void check_last_global_version_after_adding_two_views_and_after_two_consecutive_drains(
            long view1Position,
            long view2Position,
            long expectedPosition)
        {
            _viewStoreCacheInternal.Save(TestViewEnvelope1.WithGlobalVersion(GlobalVersion.Of(view1Position)));
            _viewStoreCacheInternal.Save(TestViewEnvelope2.WithGlobalVersion(GlobalVersion.Of(view2Position)));

            _manualCacheDrainer.DrainCacheUntilEmpty();
            _manualCacheDrainer.DrainCacheUntilEmpty();

            _viewStoreCacheInternal.ReadLastGlobalVersion()
                .Should()
                .Be(GlobalVersion.Of(expectedPosition));
        }
        
        [Fact]
        public void check_last_global_version_after_adding_a_view_to_final_store_without_caching()
        {
            _finalStore.Save(TestViewEnvelope1.WithGlobalVersion(GlobalVersion.Of(5)));

            _viewStoreCacheInternal.ReadLastGlobalVersion()
                .Should()
                .Be(GlobalVersion.Of(5));
        }
    }
}