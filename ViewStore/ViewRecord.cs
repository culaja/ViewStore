using System;

namespace ViewStore
{
    public sealed class ViewRecord : IEquatable<ViewRecord>
    {
        public string Id { get; }
        public IView View { get; }
        public long GlobalVersion { get; private set; }

        public ViewRecord(
            string id,
            IView view,
            long globalVersion)
        {
            Id = id;
            View = view;
            GlobalVersion = globalVersion;
        }

        public static ViewRecord NewOf(string id, IView view) => new(
            id,
            view,
            0L);
        
        public static ViewRecord EmptyWith(string id, long globalVersion) => new(
            id,
            new EmptyView(),
            globalVersion);
        
        public bool Transform<T>(long transformationGlobalVersion, Action<T> transform) where T : IView
        {
            if (GlobalVersion < transformationGlobalVersion)
            {
                transform((T)View);
                GlobalVersion = transformationGlobalVersion;
                return true;
            }

            return false;
        }
        
        public ViewRecord ImmutableTransform<T>(long transformationGlobalVersion, Func<T,T> transform) where T : IView
        {
            if (GlobalVersion < transformationGlobalVersion)
            {
                var newView = transform((T)View);
                return new ViewRecord(
                    Id,
                    newView,
                    transformationGlobalVersion);
            }

            return this;
        }

        public bool Equals(ViewRecord? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && View.Equals(other.View) && GlobalVersion.Equals(other.GlobalVersion);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is ViewRecord other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public override string ToString() => 
            $"{nameof(Id)}: {Id}, {nameof(View)}: {View}, {nameof(GlobalVersion)}: {GlobalVersion}";

        public ViewRecord WithGlobalVersion(long newGlobalVersion) => new(Id, View, newGlobalVersion);
    }
}