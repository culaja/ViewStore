using System;
using ViewStore.Abstractions;

namespace ViewStore.Postgres
{
    internal sealed class PostgresViewStoreTests : ViewStoreTests
    {
        protected override IViewStore BuildViewStore() => PostgresViewStoreBuilder.New()
            .WithConnectionString("Host=localhost;Database=ViewStore;Username=postgres;Password=postgres")
            .WithTablePath($"S{Guid.NewGuid():N}.T{Guid.NewGuid():N}")
            .ShouldAutoCreate(true)
            .Build();
    }
}