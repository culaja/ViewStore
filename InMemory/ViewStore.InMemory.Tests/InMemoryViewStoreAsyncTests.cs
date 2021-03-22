using ViewStore.Abstractions;
using ViewStore.Tests;

namespace ViewStore.InMemory
{
    public sealed class InMemoryViewStoreAsyncTests : ViewStoreAsyncTests
    {
        protected override IViewStore BuildViewStore() => new InMemoryViewStore();
    }
}