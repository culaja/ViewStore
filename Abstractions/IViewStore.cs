using System.Threading.Tasks;

namespace Abstractions
{
    public interface IViewStore
    {
        Task<T?> ReadAsync<T>(IViewId viewId) where T : IView;

        Task SaveAsync<T>(IViewId viewId, T view) where T : IView;
    }
}