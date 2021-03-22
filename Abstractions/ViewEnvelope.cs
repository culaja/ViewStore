using System;

namespace ViewStore.Abstractions
{
    public sealed class ViewEnvelope : IEquatable<ViewEnvelope>
    {
        public string Id { get; }
        public IView View { get; }
        public GlobalVersion GlobalVersion { get; private set; }

        public ViewEnvelope(
            string id,
            IView view,
            GlobalVersion globalVersion)
        {
            Id = id;
            View = view;
            GlobalVersion = globalVersion;
        }

        public bool Transform<T>(
            GlobalVersion transformationGlobalVersion,
            Action<T> transform) where T : IView
        {
            if (GlobalVersion < transformationGlobalVersion)
            {
                transform((T)View);
                GlobalVersion = transformationGlobalVersion;
                return true;
            }

            return false;
        }
        
        public ViewEnvelope ImmutableTransform<T>(
            GlobalVersion transformationGlobalVersion,
            Func<T, T> transform) where T : IView
        {
            if (GlobalVersion < transformationGlobalVersion)
            {
                var newView = transform((T)View);
                return new ViewEnvelope(
                    Id,
                    newView,
                    transformationGlobalVersion);
            }

            return this;
        }

        public bool Equals(ViewEnvelope other)
        {
            return Id == other.Id && View.Equals(other.View) && GlobalVersion.Equals(other.GlobalVersion);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is ViewEnvelope other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, View, GlobalVersion);
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(View)}: {View}, {nameof(GlobalVersion)}: {GlobalVersion}";
        }

        public ViewEnvelope WithGlobalVersion(GlobalVersion newGlobalVersion) => new(Id, View, newGlobalVersion);
    }
}