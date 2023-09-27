namespace ViewStore.Cache;

internal readonly struct DeletedViewRecord
{
    public string ViewId { get; }
    public long GlobalVersion { get; }

    public DeletedViewRecord(
        string viewId,
        long globalVersion)
    {
        ViewId = viewId;
        GlobalVersion = globalVersion;
    }
}