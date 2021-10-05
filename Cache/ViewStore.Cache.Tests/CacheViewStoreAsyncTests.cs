using System;
using System.Runtime.Caching;
using ViewStore.Abstractions;
using ViewStore.InMemory;

namespace ViewStore.Cache
{
    public sealed class CacheViewStoreAsyncTests : ViewStoreAsyncTests
    {
        protected override IViewStore BuildViewStore() =>
            ViewStoreCacheFactory.New()
                .For(new InMemoryViewStore())
                .WithCacheDrainPeriod(TimeSpan.Zero)
                .WithReadMemoryCache(new MemoryCache(Guid.NewGuid().ToString()))
                .Build();
    }
}