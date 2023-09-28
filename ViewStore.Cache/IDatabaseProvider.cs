using System.Collections.Generic;
using System.Threading.Tasks;

namespace ViewStore.Cache;

public interface IDatabaseProvider
{
    Task PrepareSchema();
    
    Task<long?> ReadLastGlobalVersionAsync();
    Task SaveLastGlobalVersionAsync(long globalVersion);
        
    Task<ViewRecord?> ReadAsync(string viewId);
        
    Task<long> UpsertAsync(IEnumerable<ViewRecord> viewRecords);
    Task<long> DeleteAsync(IEnumerable<string> viewIds);
}