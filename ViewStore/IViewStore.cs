using System.Collections.Generic;
using System.Threading.Tasks;

namespace ViewStore
{
    public interface IViewStore
    {
        Task<long?> ReadLastGlobalVersion();
        
        Task<ViewRecord?> Read(string viewId);

        void Save(ViewRecord viewRecord);
        void Save(IEnumerable<ViewRecord> viewRecords);

        void Delete(string viewId, long globalVersion = 0L);
        void Delete(IEnumerable<string> viewIds, long globalVersion = 0L);
    }
}