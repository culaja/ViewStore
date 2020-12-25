using System.Threading.Tasks;

namespace Abstractions
{
    public interface IViewStore<T> where T : IView
    {
        long? ReadGlobalVersion();
        Task<long?> ReadGlobalVersionAsync();

        T? Read(string viewId);
        Task<T?> ReadAsync(string viewId);

        void Save(T view);
        Task SaveAsync(T view);
    }
}