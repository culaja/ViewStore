using System.Threading.Tasks;

namespace Abstractions
{
    public interface IViewStorePositionTracker
    {
        long? ReadLastKnownPosition();
        Task<long?> ReadLastKnownPositionAsync();

        void StoreLastKnownPosition(long position);
        Task StoreLastKnownPositionAsync(long position);
    }
}