using System.Threading.Tasks;

namespace Abstractions
{
    public interface IViewStore
    {
        long? ReadGlobalVersion<T>() where T : IView;
        Task<long?> ReadGlobalVersionAsync<T>() where T : IView;
        
        T? Read<T>(IViewId viewId) where T : IView;
        Task<T?> ReadAsync<T>(IViewId viewId) where T : IView;

        void Save<T>(IViewId viewId, T view) where T : IView;
        Task SaveAsync<T>(IViewId viewId, T view) where T : IView;
    }
}