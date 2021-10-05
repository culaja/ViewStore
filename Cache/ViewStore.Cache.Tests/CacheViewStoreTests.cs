using System;
using ViewStore.Abstractions;
using ViewStore.InMemory;

namespace ViewStore.Cache
{
    public sealed class CacheViewStoreTests : ViewStoreTests
    {
        protected override IViewStore BuildViewStore() =>
            ViewStoreCacheFactory.New()
                .For(new InMemoryViewStore())
                .WithCacheDrainPeriod(TimeSpan.Zero)
                .Build();
    }
}