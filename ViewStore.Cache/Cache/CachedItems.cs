using System;
using System.Collections.Generic;
using System.Linq;

namespace ViewStore.Cache.Cache;

internal sealed class CachedItems
{
    private readonly Dictionary<string, ViewRecord> _addedOrUpdated = new();
    private readonly Dictionary<string, long> _deleted = new();

    public IReadOnlyCollection<ViewRecord> AddedOrUpdated => _addedOrUpdated.Values;

    public IReadOnlyCollection<DeletedViewRecord> Deleted => _deleted
        .Select(d => new DeletedViewRecord(d.Key, d.Value))
        .ToList();

    public long Count => _addedOrUpdated.Count + _deleted.Count;

    public void AddOrUpdate(ViewRecord viewRecord)
    {
        _deleted.Remove(viewRecord.Id);
        _addedOrUpdated[viewRecord.Id] = viewRecord;
    }
        
    public void AddOrUpdate(IEnumerable<ViewRecord> viewEnvelopes)
    {
        foreach (var viewEnvelope in viewEnvelopes)
        {
            _deleted.Remove(viewEnvelope.Id);
            _addedOrUpdated[viewEnvelope.Id] = viewEnvelope;    
        }
    }

    public void Remove(string viewId, long globalVersion)
    {
        _addedOrUpdated.Remove(viewId);
        _deleted[viewId] = globalVersion;
    }
        
    public void Remove(IEnumerable<string> viewIds, long globalVersion)
    {
        foreach (var viewId in viewIds)
        {
            _addedOrUpdated.Remove(viewId);
            _deleted[viewId] = globalVersion;    
        }
    }
        
    public bool TryGetValue(string viewId, out ViewRecord viewRecord, out bool isDeleted)
    {
        isDeleted = _deleted.ContainsKey(viewId);
        return _addedOrUpdated.TryGetValue(viewId, out viewRecord!);
    }

    public long? LastGlobalVersion()
    {
        if (_addedOrUpdated.Count == 0 && _deleted.Count == 0)
        {
            return null;
        }

        return Math.Max(
            _addedOrUpdated.Count > 0 ? _addedOrUpdated.Values.Max(ve => ve.GlobalVersion) : 0L,
            _deleted.Count > 0 ? _deleted.Values.Max() : 0L);
    }
}