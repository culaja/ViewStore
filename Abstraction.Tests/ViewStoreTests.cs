using FluentAssertions;
using Xunit;
using static ViewStore.Abstractions.TestView;

namespace ViewStore.Abstractions
{
    public abstract class ViewStoreTests
    {
        protected abstract IViewStore BuildViewStore();
        
        [Fact]
        public void when_view_store_is_empty_last_global_version_is_null()
        {
            var viewStore = BuildViewStore();

            viewStore.ReadLastGlobalVersion().Should().BeNull();
        }

        [Fact]
        public void after_saving_single_view_last_global_version_in_store_point_to_views_global_version()
        {
            var viewStore = BuildViewStore();
            
            viewStore.Save(TestViewEnvelope1);

            viewStore.ReadLastGlobalVersion().Should().Be(TestViewEnvelope1.GlobalVersion);
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
        public void after_saving_multiple_views_last_global_version_in_store_points_to_greatest_view_global_version(
            long view1GlobalVersionPart1,
            long view1GlobalVersionPart2,
            long view2GlobalVersionPart1,
            long view2GlobalVersionPart2,
            long expectedLastGlobalVersionPart1,
            long expectedLastGlobalVersionPart2)
        {
            var viewStore = BuildViewStore();
            
            viewStore.Save(TestViewEnvelope1.WithGlobalVersion(GlobalVersion.Of(view1GlobalVersionPart1, view1GlobalVersionPart2)));
            viewStore.Save(TestViewEnvelope2.WithGlobalVersion(GlobalVersion.Of(view2GlobalVersionPart1, view2GlobalVersionPart2)));

            viewStore.ReadLastGlobalVersion()
                .Should().Be(GlobalVersion.Of(expectedLastGlobalVersionPart1, expectedLastGlobalVersionPart2));
        }

        [Fact]
        public void saved_view_is_correctly_retrieved_from_store()
        {
            var viewStore = BuildViewStore();
            
            viewStore.Save(TestViewEnvelope1);

            viewStore.Read(TestViewEnvelope1.Id).Should().Be(TestViewEnvelope1);
        }
        
        [Fact]
        public void if_another_view_is_saved_read_returns_null()
        {
            var viewStore = BuildViewStore();
            
            viewStore.Save(TestViewEnvelope2);

            viewStore.Read(TestViewEnvelope1.Id).Should().BeNull();
        }
        
        [Fact]
        public void saving_two_views_is_working_properly()
        {
            var viewStore = BuildViewStore();
            
            viewStore.Save(TestViewEnvelope1);
            viewStore.Save(TestViewEnvelope2);

            viewStore.Read(TestViewEnvelope1.Id).Should().Be(TestViewEnvelope1);
            viewStore.Read(TestViewEnvelope2.Id).Should().Be(TestViewEnvelope2);
        }
        
        [Fact]
        public void saving_view_transformation_will_keep_only_last_version()
        {
            var viewStore = BuildViewStore();

            var transformedViewEnvelope = TestViewEnvelope1.ImmutableTransform<TestView>(
                GlobalVersion.Of(1, 0),
                testView => testView.Increment());
            
            viewStore.Save(TestViewEnvelope1);
            viewStore.Save(transformedViewEnvelope);

            viewStore.Read(TestViewEnvelope1.Id).Should().Be(transformedViewEnvelope);
        }
    }
}