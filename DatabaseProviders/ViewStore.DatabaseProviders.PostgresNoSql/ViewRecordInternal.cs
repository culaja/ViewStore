using Newtonsoft.Json;
using ViewStore.Cache;

namespace ViewStore.DatabaseProviders.PostgresNoSql;

internal sealed class ViewRecordInternal
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new() { 
        TypeNameHandling = TypeNameHandling.All
    };
    
    public string Id { get; }
    public string View { get; }
    public string ViewType { get; }
    public string ShortViewType { get; }
    public long GlobalVersion { get; }

    public ViewRecordInternal(
        string id,
        string view,
        string viewType,
        string shortViewType,
        long globalVersion)
    {
        Id = id;
        View = view;
        ViewType = viewType;
        ShortViewType = shortViewType;
        GlobalVersion = globalVersion;
    }

    public ViewRecordInternal(ViewRecord viewRecord) : this(
        viewRecord.Id,
        JsonConvert.SerializeObject(viewRecord.View, JsonSerializerSettings),
        viewRecord.View.GetType().AssemblyQualifiedName!,
        viewRecord.View.GetType().Name,
        viewRecord.GlobalVersion)
    {
    }

    public ViewRecord ToViewRecord() => new(
        Id,
        (JsonConvert.DeserializeObject(View, Type.GetType(ViewType)!) as IView)!, 
        GlobalVersion);
}