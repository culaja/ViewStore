using System;
using ViewStore.Abstractions;
using Weasel.Postgresql;

namespace ViewStore.MartenDb
{
    internal sealed class MartenDbViewStoreAsyncTests : ViewStoreAsyncTests
    {
        protected override IViewStore BuildViewStore() => MartenDbViewStoreBuilder.New()
            .WithConnectionString("host=localhost;port=8276;database=EventStore;password=dagi123;username=root")
            .WithSchemaName($"S{Guid.NewGuid():N}")
            .WithAutoCreate(AutoCreate.All)
            .Build();
    }
}