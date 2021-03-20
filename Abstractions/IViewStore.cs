using System.Threading.Tasks;

namespace ViewStore.Abstractions
{
    public interface IViewStore
    {
        GlobalVersion? ReadLastKnownPosition();
        Task<GlobalVersion?> ReadLastKnownPositionAsync();
        
        ViewEnvelope? Read(string viewId);
        Task<ViewEnvelope?> ReadAsync(string viewId);

        void Save(ViewEnvelope viewEnvelope);
        Task SaveAsync(ViewEnvelope viewEnvelope);
    }
}