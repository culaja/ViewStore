using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static ViewStore.Abstractions.TestView;

namespace ViewStore.Abstractions
{
    public abstract class ViewStoreTests
    {
        protected abstract IViewStore BuildViewStore();
        
        [Fact]
        public async Task when_view_store_is_empty_last_global_version_is_null()
        {
            var viewStore = BuildViewStore();

            (await viewStore.ReadLastGlobalVersion()).Should().BeNull();
        }

        [Fact]
        public async Task after_saving_single_view_last_global_version_in_store_point_to_views_global_version()
        {
            var viewStore = BuildViewStore();
            
            viewStore.Save(TestViewEnvelope1);

            (await viewStore.ReadLastGlobalVersion()).Should().Be(TestViewEnvelope1.GlobalVersion);
        }
        
        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 1, 1)]
        [InlineData(1, 2, 2)]
        [InlineData(2, 1, 2)]
        public async Task after_saving_multiple_views_last_global_version_in_store_points_to_greatest_view_global_version(
            long view1GlobalVersion,
            long view2GlobalVersion,
            long expectedLastGlobalVersion)
        {
            var viewStore = BuildViewStore();
            
            viewStore.Save(TestViewEnvelope1.WithGlobalVersion(view1GlobalVersion));
            viewStore.Save(TestViewEnvelope2.WithGlobalVersion(view2GlobalVersion));

            (await viewStore.ReadLastGlobalVersion())
                .Should().Be(expectedLastGlobalVersion);
        }

        [Fact]
        public async Task saved_view_is_correctly_retrieved_from_store()
        {
            var viewStore = BuildViewStore();
            
            viewStore.Save(TestViewEnvelope1);

            (await viewStore.Read(TestViewEnvelope1.Id)).Should().Be(TestViewEnvelope1);
        }
        
        [Fact]
        public async Task if_another_view_is_saved_read_returns_null()
        {
            var viewStore = BuildViewStore();
            
            viewStore.Save(TestViewEnvelope2);

            (await viewStore.Read(TestViewEnvelope1.Id)).Should().BeNull();
        }
        
        [Fact]
        public async Task saving_two_views_is_working_properly()
        {
            var viewStore = BuildViewStore();
            
            viewStore.Save(TestViewEnvelope1);
            viewStore.Save(TestViewEnvelope2);

            (await viewStore.Read(TestViewEnvelope1.Id)).Should().Be(TestViewEnvelope1);
            (await viewStore.Read(TestViewEnvelope2.Id)).Should().Be(TestViewEnvelope2);
        }
        
        [Fact]
        public async Task saving_view_transformation_will_keep_only_last_version()
        {
            var viewStore = BuildViewStore();

            var transformedViewEnvelope = TestViewEnvelope1.ImmutableTransform<TestView>(
                1L,
                testView => testView.Increment());
            
            viewStore.Save(TestViewEnvelope1);
            viewStore.Save(transformedViewEnvelope);

            (await viewStore.Read(TestViewEnvelope1.Id)).Should().Be(transformedViewEnvelope);
        }

        [Fact]
        public async Task saving_batch_of_views_works()
        {
            var viewStore = BuildViewStore();

            viewStore.Save(new [] { TestViewEnvelope1, TestViewEnvelope2} );

            (await viewStore.Read(TestViewEnvelope1.Id)).Should().Be(TestViewEnvelope1);
            (await viewStore.Read(TestViewEnvelope2.Id)).Should().Be(TestViewEnvelope2);
        }

        [Fact]
        public async Task after_deleting_an_object_it_cant_be_found_in_store()
        {
            var viewStore = BuildViewStore();
            viewStore.Save(TestViewEnvelope1);

            viewStore.Delete(TestViewEnvelope1.Id, 2L);

            (await viewStore.Read(TestViewEnvelope1.Id)).Should().BeNull();
        }
        
        [Fact]
        public async Task after_deleting_an_object_last_global_version_is_updated_correctly()
        {
            var viewStore = BuildViewStore();
            viewStore.Save(TestViewEnvelope1);

            viewStore.Delete(TestViewEnvelope1.Id, 2L);

            (await viewStore.ReadLastGlobalVersion()).Should().Be(2L);
        }
        
        [Fact]
        public async Task after_deleting_batch_of_objects_those_objects_cant_be_found_in_store()
        {
            var viewStore = BuildViewStore();
            viewStore.Save(TestViewEnvelope1);
            viewStore.Save(TestViewEnvelope2);

            viewStore.Delete(new [] { TestViewEnvelope1.Id, TestViewEnvelope2.Id }, 3L);

            (await viewStore.Read(TestViewEnvelope1.Id)).Should().BeNull();
            (await viewStore.Read(TestViewEnvelope2.Id)).Should().BeNull();
        }
        
        [Fact]
        public async Task after_deleting_batch_of_objects_last_global_version_is_updated_correctly()
        {
            var viewStore = BuildViewStore();
            viewStore.Save(TestViewEnvelope1);
            viewStore.Save(TestViewEnvelope2);

            viewStore.Delete(new [] { TestViewEnvelope1.Id, TestViewEnvelope2.Id }, 3L);

            (await viewStore.ReadLastGlobalVersion()).Should().Be(3L);
        }
    }
}