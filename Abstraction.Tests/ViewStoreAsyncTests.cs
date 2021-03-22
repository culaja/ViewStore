using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static ViewStore.Abstractions.TestView;

namespace ViewStore.Abstractions
{
    public abstract class ViewStoreAsyncTests
    {
        protected abstract IViewStore BuildViewStore();
        
        [Fact]
        public async Task when_view_store_is_empty_last_known_position_is_null()
        {
            var viewStore = BuildViewStore();

            (await viewStore.ReadLastKnownPositionAsync()).Should().BeNull();
        }

        [Fact]
        public async Task after_saving_single_view_last_known_position_point_to_views_position()
        {
            var viewStore = BuildViewStore();
            
            await viewStore.SaveAsync(TestViewEnvelope1);

            (await viewStore.ReadLastKnownPositionAsync()).Should().Be(TestViewEnvelope1.GlobalVersion);
        }
        
        [Theory]
        [InlineData(0, 0,   0, 0,   0, 0)]
        [InlineData(0, 1,   0, 1,   0, 1)]
        [InlineData(0, 1,   0, 2,   0, 2)]
        [InlineData(0, 2,   0, 1,   0, 2)]
        [InlineData(1, 0,   1, 0,   1, 0)]
        [InlineData(1, 0,   2, 0,   2, 0)]
        [InlineData(2, 0,   1, 0,   2, 0)]
        [InlineData(6, 3,   5, 9,   6, 3)]
        [InlineData(6, 3,   6, 2,   6, 3)]
        [InlineData(6, 3,   6, 3,   6, 3)]
        [InlineData(6, 3,   7, 2,   7, 2)]
        [InlineData(6, 3,   7, 7,   7, 7)]
        public async Task after_saving_multiple_views_last_known_position_points_to_greatest_view_position(
            long view1GlobalPositionPart1,
            long view1GlobalPositionPart2,
            long view2GlobalPositionPart1,
            long view2GlobalPositionPart2,
            long expectedLastKnownPositionPart1,
            long expectedLastKnownPositionPart2)
        {
            var viewStore = BuildViewStore();
            
            await viewStore.SaveAsync(TestViewEnvelope1.WithGlobalVersion(GlobalVersion.Of(view1GlobalPositionPart1, view1GlobalPositionPart2)));
            await viewStore.SaveAsync(TestViewEnvelope2.WithGlobalVersion(GlobalVersion.Of(view2GlobalPositionPart1, view2GlobalPositionPart2)));

            (await viewStore.ReadLastKnownPositionAsync())
                .Should().Be(GlobalVersion.Of(expectedLastKnownPositionPart1, expectedLastKnownPositionPart2));
        }

        [Fact]
        public async Task saved_view_is_correctly_retrieved_from_store()
        {
            var viewStore = BuildViewStore();
            
            await viewStore.SaveAsync(TestViewEnvelope1);

            (await viewStore.ReadAsync(TestViewEnvelope1.Id)).Should().Be(TestViewEnvelope1);
        }
        
        [Fact]
        public async Task if_another_view_is_saved_read_returns_null()
        {
            var viewStore = BuildViewStore();
            
            await viewStore.SaveAsync(TestViewEnvelope2);

            (await viewStore.ReadAsync(TestViewEnvelope1.Id)).Should().BeNull();
        }
        
        [Fact]
        public async Task saving_two_views_is_working_properly()
        {
            var viewStore = BuildViewStore();
            
            await viewStore.SaveAsync(TestViewEnvelope1);
            await viewStore.SaveAsync(TestViewEnvelope2);

            (await viewStore.ReadAsync(TestViewEnvelope1.Id)).Should().Be(TestViewEnvelope1);
            (await viewStore.ReadAsync(TestViewEnvelope2.Id)).Should().Be(TestViewEnvelope2);
        }
        
        [Fact]
        public async Task saving_view_transformation_will_keep_only_last_version()
        {
            var viewStore = BuildViewStore();

            var transformedViewEnvelope = TestViewEnvelope1.ImmutableTransform<TestView>(
                GlobalVersion.Of(1, 0),
                testView => testView.Increment());
            
            await viewStore.SaveAsync(TestViewEnvelope1);
            await viewStore.SaveAsync(transformedViewEnvelope);

            (await viewStore.ReadAsync(TestViewEnvelope1.Id)).Should().Be(transformedViewEnvelope);
        }
    }
}