using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ViewStore;

namespace Tests;

public sealed class InMemoryDatabaseProvider : IDatabaseProvider
{
    private const string LastGlobalVersionViewId = "LastGlobalVersionViewId-b24ae98262724d27bd8e31c34ff11f1a";
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<string, ViewRecord> _dictionary = new();

    public Task<long?> ReadLastGlobalVersionAsync()
    {
        _lock.EnterReadLock();
        try
        {
            long? version = _dictionary.Count > 0
                ? _dictionary.Values.Max(v => v.GlobalVersion)
                : null;
            return Task.FromResult(version);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public Task SaveLastGlobalVersionAsync(long globalVersion)
    {
        _lock.EnterWriteLock();
        try
        {
            _dictionary[LastGlobalVersionViewId] = ViewRecord.EmptyWith(LastGlobalVersionViewId, globalVersion);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    public Task<ViewRecord?> ReadAsync(string viewId)
    {
        _lock.EnterReadLock();
        try
        {
            if (_dictionary.TryGetValue(viewId, out var viewRecord))
            {
                return Task.FromResult(viewRecord)!;
            }

            return Task.FromResult<ViewRecord?>(default);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
        
    public Task<long> UpsertAsync(IEnumerable<ViewRecord> viewRecords)
    {
        var upsertCount = 0L;

        _lock.EnterWriteLock();
        try
        {
            foreach (var viewRecord in viewRecords)
            {
                _dictionary[viewRecord.Id] = viewRecord;
                upsertCount++;
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.FromResult(upsertCount);
    }

    public Task<long> DeleteAsync(IEnumerable<string> viewIds)
    {
        var deleteCount = 0L;

        _lock.EnterWriteLock();
        try
        {
            foreach (var viewId in viewIds)
            {
                if (_dictionary.Remove(viewId))
                {
                    deleteCount++;
                }
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.FromResult(deleteCount);
    }
}