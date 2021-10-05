using System;
using Marten;
using ViewStore.Abstractions;

namespace ViewStore.MartenDb
{
    internal sealed class MartenDbViewStoreAsyncTests : ViewStoreAsyncTests
    {
        protected override IViewStore BuildViewStore() => MartenDbViewStoreBuilder.New()
            .WithConnectionString("host=localhost;port=8276;database=EventStore;password=dagi123;username=root")
            .WithSchemaName($"S{Guid.NewGuid():N}")
            .Build();
    }
}