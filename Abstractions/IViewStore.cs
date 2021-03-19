using System.Threading.Tasks;

namespace ViewStore.Abstractions
{
    public interface IViewStore
    {
        GlobalVersion? ReadLastKnownPosition();
        Task<GlobalVersion?> ReadLastKnownPositionAsync();
        
        T? Read<T>(string viewId) where T : IView;
        Task<T?> ReadAsync<T>(string viewId) where T : IView;

        void Save(IView view);
        Task SaveAsync(IView view);
    }
}