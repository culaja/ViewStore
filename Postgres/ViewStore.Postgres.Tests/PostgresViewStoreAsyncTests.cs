﻿using System;
using ViewStore.Abstractions;

namespace ViewStore.Postgres
{
    internal sealed class PostgresViewStoreAsyncTests : ViewStoreAsyncTests
    {
        protected override IViewStore BuildViewStore() => PostgresViewStoreBuilder.New()
            .WithConnectionString("Host=localhost;Database=ViewStore;Username=postgres;Password=o")
            .WithTablePath($"S{Guid.NewGuid():N}.T{Guid.NewGuid():N}")
            .ShouldAutoCreate(true)
            .Build();
    }
}