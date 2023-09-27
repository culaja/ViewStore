using System.Threading.Tasks;
using FluentAssertions;
using ViewStore.Cache;
using Xunit;
using static ViewStore.Abstractions.TestView;

namespace Tests
{
    public sealed class FetchingLastGlobalVersionFromCacheTests
    {
        private readonly InMemoryDatabaseProvider _databaseProvider = new();
        private readonly OutgoingCache _outgoingCache = new(1000, null);
        private readonly ManualCacheDrainer _manualCacheDrainer;
        private readonly ViewStoreCacheInternal _viewStoreCacheInternal;

        public FetchingLastGlobalVersionFromCacheTests()
        {
            _manualCacheDrainer = new ManualCacheDrainer(_databaseProvider, _outgoingCache, 10);
            _viewStoreCacheInternal = new ViewStoreCacheInternal(_databaseProvider, _outgoingCache);
        }
        
        /// <remarks>
        /// ----- Current cache -----*----- Drained cache -----*----- Database -----
        ///           V1, V2
        /// -------------------------*-------------------------*---------------------- 
        /// </remarks>
        [Theory]
        [InlineData(2, 1, 2)]
        [InlineData(1,5, 5)]
        public async Task check_last_global_version_after_adding_two_views_and_when_drain_is_not_triggered(
            long view1Position,
            long view2Position,
            long expectedPosition)
        {
            _viewStoreCacheInternal.Save(TestViewEnvelope1.WithGlobalVersion(view1Position));
            _viewStoreCacheInternal.Save(TestViewEnvelope2.WithGlobalVersion(view2Position));

            (await _viewStoreCacheInternal.ReadLastGlobalVersion())
                .Should()
                .Be(expectedPosition);
        }
        
        
        /// <remarks>
        /// ----- Current cache -----*----- Drained cache -----*----- Database -----
        ///                                    V1 V2                    V1 V2
        /// -------------------------*-------------------------*---------------------- 
        /// </remarks>
        [Theory]
        [InlineData(2, 1, 2)]
        [InlineData(1,5, 5)]
        public async Task check_last_global_version_after_adding_two_views_and_when_drain_is_triggered(
            long view1Position,
            long view2Position,
            long expectedPosition)
        {
            _viewStoreCacheInternal.Save(TestViewEnvelope1.WithGlobalVersion(view1Position));
            _viewStoreCacheInternal.Save(TestViewEnvelope2.WithGlobalVersion(view2Position));

            _manualCacheDrainer.DrainCacheUntilEmpty();

            (await _viewStoreCacheInternal.ReadLastGlobalVersion())
                .Should()
                .Be(expectedPosition);
        }
        
        /// <remarks>
        /// ----- Current cache -----*----- Drained cache -----*----- Database -----
        ///              V2                       V1                      V1
        /// -------------------------*-------------------------*---------------------- 
        /// </remarks>
        [Theory]
        [InlineData(2, 1, 2)]
        [InlineData(1,5, 5)]
        public async Task check_last_global_version_after_adding_two_views_and_when_drain_is_triggered_between_two_saves(
            long view1Position,
            long view2Position,
            long expectedPosition)
        {
            _viewStoreCacheInternal.Save(TestViewEnvelope1.WithGlobalVersion(view1Position));
            _manualCacheDrainer.DrainCacheUntilEmpty();
            _viewStoreCacheInternal.Save(TestViewEnvelope2.WithGlobalVersion(view2Position));

            (await _viewStoreCacheInternal.ReadLastGlobalVersion())
                .Should()
                .Be(expectedPosition);
        }
        
        /// <remarks>
        /// ----- Current cache -----*----- Drained cache -----*----- Database -----
        ///                                                             V1, V2
        /// -------------------------*-------------------------*---------------------- 
        /// </remarks>
        [Theory]
        [InlineData(2, 1, 2)]
        [InlineData(1,5, 5)]
        public async Task check_last_global_version_after_adding_two_views_and_after_two_consecutive_drains(
            long view1Position,
            long view2Position,
            long expectedPosition)
        {
            _viewStoreCacheInternal.Save(TestViewEnvelope1.WithGlobalVersion(view1Position));
            _viewStoreCacheInternal.Save(TestViewEnvelope2.WithGlobalVersion(view2Position));

            _manualCacheDrainer.DrainCacheUntilEmpty();
            _manualCacheDrainer.DrainCacheUntilEmpty();

            (await _viewStoreCacheInternal.ReadLastGlobalVersion())
                .Should()
                .Be(expectedPosition);
        }
        
        [Fact]
        public async Task check_last_global_version_after_adding_a_view_to_final_store_without_caching()
        {
            await _databaseProvider.SaveLastGlobalVersionAsync(5);

            (await _viewStoreCacheInternal.ReadLastGlobalVersion())
                .Should()
                .Be(5);
        }
    }
}