using ViewStore.Abstractions;
using ViewStore.Tests;

namespace ViewStore.InMemory
{
    public sealed class InMemoryViewStoreTests : ViewStoreTests
    {
        protected override IViewStore BuildViewStore() => new InMemoryViewStore();
    }
}