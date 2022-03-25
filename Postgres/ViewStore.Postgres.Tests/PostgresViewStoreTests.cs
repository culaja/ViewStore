using System;
using ViewStore.Abstractions;

namespace ViewStore.Postgres
{
    internal sealed class PostgresViewStoreTests : ViewStoreTests
    {
        protected override IViewStore BuildViewStore() => PostgresViewStoreBuilder.New()
            .WithConnectionString("Host=localhost;Port=8276;Database=ViewStore;Username=postgres;Password=postgres")
            .WithTablePath($"S{Guid.NewGuid():N}.T{Guid.NewGuid():N}")
            .ShouldAutoCreate(true)
            .Build();
    }
}