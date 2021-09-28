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
        
        public ViewEnvelope(
            string id,
            IView view,
            GlobalVersion globalVersion,
            MetaData metaData)
        {
            Id = id;
            View = view;
            GlobalVersion = globalVersion;
            MetaData = metaData;
        }

        public ViewEnvelope(
            string id,
            IView view,
            GlobalVersion globalVersion)
        {
            Id = id;
            View = view;
            GlobalVersion = globalVersion;
            MetaData = new MetaData(new Dictionary<string, string>());
        }

        public ViewEnvelope(string id, IView view) : this(
            id, 
            view, 
            GlobalVersion.Start,
            new MetaData(new Dictionary<string, string>()))
        {
        }
        
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

        public bool Equals(ViewEnvelope other) => 
            Id == other.Id && View.Equals(other.View) && GlobalVersion.Equals(other.GlobalVersion);

        public override bool Equals(object? obj) => 
            ReferenceEquals(this, obj) || obj is ViewEnvelope other && Equals(other);

        public override int GetHashCode() => Id.GetHashCode();

        public override string ToString() => 
            $"{nameof(Id)}: {Id}, {nameof(View)}: {View}, {nameof(GlobalVersion)}: {GlobalVersion}";

        public ViewEnvelope WithGlobalVersion(GlobalVersion newGlobalVersion) => new(Id, View, newGlobalVersion, MetaData);
    }
}