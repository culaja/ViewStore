using System;
using System.Runtime.Caching;
using ViewStore.Abstractions;
using ViewStore.InMemory;

namespace ViewStore.ReadThroughCache
{
    public sealed class ReadThroughViewStoreCacheAsyncTests : ViewStoreAsyncTests
    {
        protected override IViewStore BuildViewStore() =>
            new ReadThroughViewStoreCache(
                new MemoryCache(Guid.NewGuid().ToString()), 
                TimeSpan.FromSeconds(1), 
                new InMemoryViewStore());
    }
}