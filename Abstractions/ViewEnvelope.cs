using System;
using System.Collections.Generic;

namespace ViewStore.Abstractions
{
    public sealed class ViewEnvelope : IEquatable<ViewEnvelope>
    {
        public string Id { get; }
        public IView View { get; }
        public GlobalVersion GlobalVersion { get; private set; }
        public MetaData MetaData { get; }
        public string? TenantId { get; }
        public DateTime? CreatedAt { get; }

        public ViewEnvelope(
            string id,
            IView view,
            GlobalVersion globalVersion,
            MetaData metaData,
            string? tenantId = null,
            DateTime? createdAt = null)
        {
            Id = id;
            View = view;
            GlobalVersion = globalVersion;
            MetaData = metaData;
            TenantId = tenantId;
            CreatedAt = createdAt;
        }

        public static ViewEnvelope NewOf(string id, IView view) => new(
            id,
            view,
            GlobalVersion.Start,
            new MetaData(new Dictionary<string, string>()));
        
        public static ViewEnvelope EmptyWith(string id, GlobalVersion globalVersion) => new(
            id,
            new EmptyView(),
            globalVersion,
            new MetaData(new Dictionary<string, string>()));
        
        public bool Transform<T>(
            GlobalVersion transformationGlobalVersion,
            Action<T> transform) where T : IView
        {
            return Transform<T>(transformationGlobalVersion, (v, _) => transform(v));
        }

        public bool Transform<T>(
            GlobalVersion transformationGlobalVersion,
            Action<T, MetaData> transform) where T : IView
        {
            if (GlobalVersion < transformationGlobalVersion)
            {
                transform((T)View, MetaData);
                GlobalVersion = transformationGlobalVersion;
                return true;
            }

            return false;
        }
        
        public ViewEnvelope ImmutableTransform<T>(
            GlobalVersion transformationGlobalVersion,
            Func<T, T> transform) where T : IView
        {
            return ImmutableTransform<T>(transformationGlobalVersion, (v, _) => transform(v));
        }
        
        public ViewEnvelope ImmutableTransform<T>(
            GlobalVersion transformationGlobalVersion,
            Func<T, MetaData, T> transform) where T : IView
        {
            if (GlobalVersion < transformationGlobalVersion)
            {
                var newView = transform((T)View, MetaData);
                return new ViewEnvelope(
                    Id,
                    newView,
                    transformationGlobalVersion,
                    MetaData);
            }

            return this;
        }

        public bool Equals(ViewEnvelope? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && View.Equals(other.View) && GlobalVersion.Equals(other.GlobalVersion) && TenantId == other.TenantId && Nullable.Equals(CreatedAt, other.CreatedAt);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is ViewEnvelope other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, TenantId, CreatedAt);
        }

        public override string ToString() => 
            $"{nameof(Id)}: {Id}, {nameof(View)}: {View}, {nameof(GlobalVersion)}: {GlobalVersion}";

        public ViewEnvelope WithGlobalVersion(GlobalVersion newGlobalVersion) => new(Id, View, newGlobalVersion, MetaData);
    }
}