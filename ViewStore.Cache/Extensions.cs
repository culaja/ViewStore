using System;

namespace ViewStore.Cache;

public static class Extensions
{
    public static ViewRecord ToRecord<V>(this V view, Func<V, string> idProvider) where V: IView => 
        ViewRecord.NewOf(idProvider(view), view);
}