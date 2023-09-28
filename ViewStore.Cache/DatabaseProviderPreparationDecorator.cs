using System.Collections.Generic;
using System.Threading.Tasks;

namespace ViewStore.Cache;

internal sealed class DatabaseProviderPreparationDecorator : IDatabaseProvider
{
    private readonly IDatabaseProvider _next;
    private bool _isSchemaCreated;

    public DatabaseProviderPreparationDecorator(IDatabaseProvider next)
    {
        _next = next;
    }

    public async Task PrepareSchema()
    {
        await PrepareSchemaIfNeeded();
    }

    public async Task<long?> ReadLastGlobalVersionAsync()
    {
        await PrepareSchemaIfNeeded();
        return await _next.ReadLastGlobalVersionAsync();
    }

    public async Task SaveLastGlobalVersionAsync(long globalVersion)
    {
        await PrepareSchemaIfNeeded();
        await _next.SaveLastGlobalVersionAsync(globalVersion);
    }

    public async Task<ViewRecord?> ReadAsync(string viewId)
    {
        await PrepareSchemaIfNeeded();
        return await _next.ReadAsync(viewId);
    }

    public async Task<long> UpsertAsync(IEnumerable<ViewRecord> viewRecords)
    {
        await PrepareSchemaIfNeeded();
        return await _next.UpsertAsync(viewRecords);
    }

    public async Task<long> DeleteAsync(IEnumerable<string> viewIds)
    {
        await PrepareSchemaIfNeeded();
        return await _next.DeleteAsync(viewIds);
    }

    private async Task PrepareSchemaIfNeeded()
    {
        if (!_isSchemaCreated)
        {
            await _next.PrepareSchema();
            _isSchemaCreated = true;
        }
    }
}