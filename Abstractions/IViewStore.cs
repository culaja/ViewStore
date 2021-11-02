using System.Collections.Generic;
using System.Threading.Tasks;

namespace ViewStore.Abstractions
{
    public interface IViewStore
    {
        GlobalVersion? ReadLastGlobalVersion();
        Task<GlobalVersion?> ReadLastGlobalVersionAsync();
        
        ViewEnvelope? Read(string viewId);
        Task<ViewEnvelope?> ReadAsync(string viewId);

        void Save(ViewEnvelope viewEnvelope);
        Task SaveAsync(ViewEnvelope viewEnvelope);

        void Save(IEnumerable<ViewEnvelope> viewEnvelopes);
        Task SaveAsync(IEnumerable<ViewEnvelope> viewEnvelopes);

        void Delete(string viewId, GlobalVersion globalVersion);
        Task DeleteAsync(string viewId, GlobalVersion globalVersion);
        
        void Delete(IEnumerable<string> viewIds, GlobalVersion globalVersion);
        Task DeleteAsync(IEnumerable<string> viewIds, GlobalVersion globalVersion);
    }
}