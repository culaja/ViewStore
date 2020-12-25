using System.Threading.Tasks;

namespace Abstractions
{
    public interface IViewStore<T> where T : IView
    {
        T? Read(string viewId);
        Task<T?> ReadAsync(string viewId);

        void Save(T view);
        Task SaveAsync(T view);
    }
}