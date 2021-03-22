using ViewStore.Abstractions;

namespace ViewStore.InMemory
{
    public sealed class InMemoryViewStoreTests : ViewStoreTests
    {
        protected override IViewStore BuildViewStore() => new InMemoryViewStore();
    }
}