namespace Abstractions
{
    public interface IView
    {
        string Id { get; } 
        long GlobalVersion { get; }
    }
}