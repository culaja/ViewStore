using System.Threading.Tasks;
using FluentAssertions;
using ViewStore.Abstractions;
using Xunit;

namespace ViewStore.Tests
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
            
            await viewStore.SaveAsync(TestView.TestViewEnvelope1);

            (await viewStore.ReadLastKnownPositionAsync()).Should().Be(TestView.TestViewEnvelope1.GlobalVersion);
        }
        
        [Fact]
        public async Task after_saving_multiple_views_last_known_position_point_to__greatest_view_position()
        {
            var viewStore = BuildViewStore();
            
            await viewStore.SaveAsync(TestView.TestViewEnvelope1);
            await viewStore.SaveAsync(TestView.TestViewEnvelope2);

            (await viewStore.ReadLastKnownPositionAsync()).Should().Be(TestView.TestViewEnvelope2.GlobalVersion);
        }

        [Fact]
        public async Task saved_view_is_correctly_retrieved_from_store()
        {
            var viewStore = BuildViewStore();
            
            await viewStore.SaveAsync(TestView.TestViewEnvelope1);

            (await viewStore.ReadAsync(TestView.TestViewEnvelope1.Id)).Should().Be(TestView.TestViewEnvelope1);
        }
        
        [Fact]
        public async Task if_another_view_is_saved_read_returns_null()
        {
            var viewStore = BuildViewStore();
            
            await viewStore.SaveAsync(TestView.TestViewEnvelope2);

            (await viewStore.ReadAsync(TestView.TestViewEnvelope1.Id)).Should().BeNull();
        }
        
        [Fact]
        public async Task saving_two_views_is_working_properly()
        {
            var viewStore = BuildViewStore();
            
            await viewStore.SaveAsync(TestView.TestViewEnvelope1);
            await viewStore.SaveAsync(TestView.TestViewEnvelope2);

            (await viewStore.ReadAsync(TestView.TestViewEnvelope1.Id)).Should().Be(TestView.TestViewEnvelope1);
            (await viewStore.ReadAsync(TestView.TestViewEnvelope2.Id)).Should().Be(TestView.TestViewEnvelope2);
        }
        
        [Fact]
        public async Task saving_view_transformation_will_keep_only_last_version()
        {
            var viewStore = BuildViewStore();

            var transformedViewEnvelope = TestView.TestViewEnvelope1.ImmutableTransform<TestView>(
                GlobalVersion.Of(1, 0),
                testView => testView.Increment());
            
            await viewStore.SaveAsync(TestView.TestViewEnvelope1);
            await viewStore.SaveAsync(transformedViewEnvelope);

            (await viewStore.ReadAsync(TestView.TestViewEnvelope1.Id)).Should().Be(transformedViewEnvelope);
        }
    }
}