namespace ViewStore.Abstractions
{
    public interface IView
    {
        string Id { get; } 
        GlobalVersion GlobalVersion { get; }
    }
}