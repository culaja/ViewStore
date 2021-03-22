using ViewStore.Abstractions;
using ViewStore.InMemory;

namespace ViewStore.WriteBehindCache
{
    public sealed class WriteBehindCacheViewStoreTests : ViewStoreTests
    {
        protected override IViewStore BuildViewStore() =>
            ViewStoreCacheFactory.New()
                .For(new InMemoryViewStore())
                .Build();
    }
}