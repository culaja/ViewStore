using System;

namespace ViewStore;

public static class Extensions
{
    public static ViewRecord ToRecord<V>(this V view, Func<V, string> idProvider) where V: IView => 
        ViewRecord.NewOf(idProvider(view), view);
}