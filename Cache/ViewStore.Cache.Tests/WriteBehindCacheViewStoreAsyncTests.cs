using System;
using ViewStore.Abstractions;
using ViewStore.InMemory;

namespace ViewStore.Cache
{
    public sealed class WriteBehindCacheViewStoreAsyncTests : ViewStoreAsyncTests
    {
        protected override IViewStore BuildViewStore() =>
            ViewStoreCacheFactory.New()
                .For(new InMemoryViewStore())
                .WithCacheDrainPeriod(TimeSpan.Zero)
                .Build();
    }
}