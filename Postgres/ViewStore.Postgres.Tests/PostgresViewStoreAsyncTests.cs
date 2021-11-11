using System;
using ViewStore.Abstractions;

namespace ViewStore.Postgres
{
    public sealed class PostgresViewStoreAsyncTests : ViewStoreAsyncTests
    {
        protected override IViewStore BuildViewStore() => PostgresViewStoreBuilder.New()
            .WithConnectionString("Host=192.168.1.5;Database=Accounting;Username=postgres;Password=o")
            .WithTablePath($"S{Guid.NewGuid():N}.T{Guid.NewGuid():N}")
            .ShouldAutoCreate(true)
            .Build();
    }
}