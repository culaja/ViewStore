using System.Threading.Tasks;

namespace Abstractions
{
    public interface IViewStorePositionTracker<T> where T : IView
    {
        long ReadLastGlobalVersion();
        Task<long> ReadLastGlobalVersionAsync();

        void StoreLastGlobalVersionFrom(T view);
        Task StoreLastGlobalVersionFromAsync(T view);
    }
}