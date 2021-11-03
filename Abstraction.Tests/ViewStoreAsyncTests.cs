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
        public async Task when_view_store_is_empty_last_global_version_is_null()
        {
            var viewStore = BuildViewStore();

            (await viewStore.ReadLastGlobalVersionAsync()).Should().BeNull();
        }

        [Fact]
        public async Task after_saving_single_view_last_global_version_in_store_point_to_views_global_version()
        {
            var viewStore = BuildViewStore();
            
            await viewStore.SaveAsync(TestViewEnvelope1);

            (await viewStore.ReadLastGlobalVersionAsync()).Should().Be(TestViewEnvelope1.GlobalVersion);
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
            
            await viewStore.SaveAsync(TestViewEnvelope1.WithGlobalVersion(GlobalVersion.Of(view1GlobalVersion)));
            await viewStore.SaveAsync(TestViewEnvelope2.WithGlobalVersion(GlobalVersion.Of(view2GlobalVersion)));

            (await viewStore.ReadLastGlobalVersionAsync())
                .Should().Be(GlobalVersion.Of(expectedLastGlobalVersion));
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
                GlobalVersion.Of(1),
                testView => testView.Increment());
            
            await viewStore.SaveAsync(TestViewEnvelope1);
            await viewStore.SaveAsync(transformedViewEnvelope);

            (await viewStore.ReadAsync(TestViewEnvelope1.Id)).Should().Be(transformedViewEnvelope);
        }
        
        [Fact]
        public async Task saving_batch_of_views_works()
        {
            var viewStore = BuildViewStore();

            await viewStore.SaveAsync(new [] { TestViewEnvelope1, TestViewEnvelope2} );

            (await viewStore.ReadAsync(TestViewEnvelope1.Id)).Should().Be(TestViewEnvelope1);
            (await viewStore.ReadAsync(TestViewEnvelope2.Id)).Should().Be(TestViewEnvelope2);
        }
        
        [Fact]
        public async Task metadata_is_correctly_persisted()
        {
            var expectedViewEnvelope = NewTestViewEnvelopeWith(10);
            expectedViewEnvelope.MetaData.Set("CorrelationId", "SomeCorrelationId");
            expectedViewEnvelope.MetaData.Set("CausationId", "SomeCausationId");
            
            var viewStore = BuildViewStore();
            await viewStore.SaveAsync(expectedViewEnvelope);

            var actualViewEnvelope = await viewStore.ReadAsync(expectedViewEnvelope.Id);
            actualViewEnvelope!.MetaData.Get("CorrelationId").Should().Be("SomeCorrelationId");
            actualViewEnvelope.MetaData.Get("CausationId").Should().Be("SomeCausationId");
            actualViewEnvelope.MetaData.Get("InvalidKey").Should().BeNull();
        }
        
        [Fact]
        public async Task after_deleting_an_object_it_cant_be_found_in_store()
        {
            var viewStore = BuildViewStore();
            await viewStore.SaveAsync(TestViewEnvelope1);

            await viewStore.DeleteAsync(TestViewEnvelope1.Id, GlobalVersion.Of(2));

            (await viewStore.ReadAsync(TestViewEnvelope1.Id)).Should().BeNull();
        }
        
        [Fact]
        public async Task after_deleting_an_object_last_global_version_is_updated_correctly()
        {
            var viewStore = BuildViewStore();
            await viewStore.SaveAsync(TestViewEnvelope1);

            await viewStore.DeleteAsync(TestViewEnvelope1.Id, GlobalVersion.Of(2));

            (await viewStore.ReadLastGlobalVersionAsync()).Should().Be(GlobalVersion.Of(2));
        }
        
        [Fact]
        public async Task after_deleting_batch_of_objects_those_objects_cant_be_found_in_store()
        {
            var viewStore = BuildViewStore();
            await viewStore.SaveAsync(TestViewEnvelope1);
            await viewStore.SaveAsync(TestViewEnvelope2);

            await viewStore.DeleteAsync(new [] { TestViewEnvelope1.Id, TestViewEnvelope2.Id }, GlobalVersion.Of(3));

            (await viewStore.ReadAsync(TestViewEnvelope1.Id)).Should().BeNull();
            (await viewStore.ReadAsync(TestViewEnvelope2.Id)).Should().BeNull();
        }
        
        [Fact]
        public async Task after_deleting_batch_of_objects_last_global_version_is_updated_correctly()
        {
            var viewStore = BuildViewStore();
            await viewStore.SaveAsync(TestViewEnvelope1);
            await viewStore.SaveAsync(TestViewEnvelope2);

            await viewStore.DeleteAsync(new [] { TestViewEnvelope1.Id, TestViewEnvelope2.Id }, GlobalVersion.Of(3));

            (await viewStore.ReadLastGlobalVersionAsync()).Should().Be(GlobalVersion.Of(3));
        }
    }
}