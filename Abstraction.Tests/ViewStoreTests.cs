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
        [InlineData(0, 0, 0)]
        [InlineData(1, 1, 1)]
        [InlineData(1, 2, 2)]
        [InlineData(2, 1, 2)]
        public void after_saving_multiple_views_last_global_version_in_store_points_to_greatest_view_global_version(
            long view1GlobalVersion,
            long view2GlobalVersion,
            long expectedLastGlobalVersion)
        {
            var viewStore = BuildViewStore();
            
            viewStore.Save(TestViewEnvelope1.WithGlobalVersion(GlobalVersion.Of(view1GlobalVersion)));
            viewStore.Save(TestViewEnvelope2.WithGlobalVersion(GlobalVersion.Of(view2GlobalVersion)));

            viewStore.ReadLastGlobalVersion()
                .Should().Be(GlobalVersion.Of(expectedLastGlobalVersion));
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
                GlobalVersion.Of(1),
                testView => testView.Increment());
            
            viewStore.Save(TestViewEnvelope1);
            viewStore.Save(transformedViewEnvelope);

            viewStore.Read(TestViewEnvelope1.Id).Should().Be(transformedViewEnvelope);
        }

        [Fact]
        public void saving_batch_of_views_works()
        {
            var viewStore = BuildViewStore();

            viewStore.Save(new [] { TestViewEnvelope1, TestViewEnvelope2} );

            viewStore.Read(TestViewEnvelope1.Id).Should().Be(TestViewEnvelope1);
            viewStore.Read(TestViewEnvelope2.Id).Should().Be(TestViewEnvelope2);
        }

        [Fact]
        public void metadata_is_correctly_persisted()
        {
            var expectedViewEnvelope = NewTestViewEnvelopeWith(10);
            expectedViewEnvelope.MetaData.Set("CorrelationId", "SomeCorrelationId");
            expectedViewEnvelope.MetaData.Set("CausationId", "SomeCausationId");
            
            var viewStore = BuildViewStore();
            viewStore.Save(expectedViewEnvelope);

            var actualViewEnvelope = viewStore.Read(expectedViewEnvelope.Id);
            actualViewEnvelope!.MetaData.Get("CorrelationId").Should().Be("SomeCorrelationId");
            actualViewEnvelope.MetaData.Get("CausationId").Should().Be("SomeCausationId");
            actualViewEnvelope.MetaData.Get("InvalidKey").Should().BeNull();
        }

        [Fact]
        public void after_deleting_an_object_it_cant_be_found_in_store()
        {
            var viewStore = BuildViewStore();
            viewStore.Save(TestViewEnvelope1);

            viewStore.Delete(TestViewEnvelope1.Id, GlobalVersion.Of(2));

            viewStore.Read(TestViewEnvelope1.Id).Should().BeNull();
        }
        
        [Fact]
        public void after_deleting_batch_of_objects_those_objects_cant_be_found_in_store()
        {
            var viewStore = BuildViewStore();
            viewStore.Save(TestViewEnvelope1);
            viewStore.Save(TestViewEnvelope2);

            viewStore.Delete(new [] { TestViewEnvelope1.Id, TestViewEnvelope2.Id }, GlobalVersion.Of(3));

            viewStore.Read(TestViewEnvelope1.Id).Should().BeNull();
            viewStore.Read(TestViewEnvelope2.Id).Should().BeNull();
        }
    }
}