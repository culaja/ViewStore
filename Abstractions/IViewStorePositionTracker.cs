using System.Threading.Tasks;

namespace Abstractions
{
    public interface IViewStorePositionTracker
    {
        long? ReadLastGlobalVersion();
        Task<long?> ReadLastGlobalVersionAsync();

        void StoreLastGlobalVersion(long globalPosition);
        Task StoreLastGlobalVersionFromAsync(long globalPosition);
    }
}