using System;
using System.Runtime.Caching;
using ViewStore.Abstractions;
using ViewStore.InMemory;

namespace ViewStore.Cache
{
    public sealed class CacheViewStoreTests : ViewStoreTests, IDisposable
    {
        private readonly ViewStoreCache _cache;

        public CacheViewStoreTests()
        {
            _cache = ViewStoreCacheBuilder.New()
                .For(new InMemoryViewStore())
                .WithCacheDrainPeriod(TimeSpan.Zero)
                .WithReadMemoryCache(new MemoryCache(Guid.NewGuid().ToString()))
                .Build();
        }

        protected override IViewStore BuildViewStore() => _cache;

        public void Dispose()
        {
            _cache.Dispose();
        }
    }
}