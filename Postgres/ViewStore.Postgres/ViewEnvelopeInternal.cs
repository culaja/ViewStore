using System;
using Newtonsoft.Json;
using ViewStore.Abstractions;

namespace ViewStore.Postgres
{
    internal sealed class ViewEnvelopeInternal
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new() { 
            TypeNameHandling = TypeNameHandling.All
        };
    
        public string Id { get; }
        public string View { get; }
        public string ViewType { get; }
        public string ShortViewType { get; }
        public string Metadata { get; }
        public long GlobalVersion { get; }
        public string TenantId { get; }
        public DateTime CreatedAt { get; }

        public ViewEnvelopeInternal(
            string id,
            string view,
            string viewType,
            string shortViewType,
            string metadata,
            long globalVersion,
            string tenantId,
            DateTime createdAt)
        {
            Id = id;
            View = view;
            ViewType = viewType;
            ShortViewType = shortViewType;
            Metadata = metadata;
            GlobalVersion = globalVersion;
            TenantId = tenantId;
            CreatedAt = createdAt;
        }

        public ViewEnvelopeInternal(ViewEnvelope viewEnvelope) : this(
            viewEnvelope.Id,
            JsonConvert.SerializeObject(viewEnvelope.View, JsonSerializerSettings),
            viewEnvelope.View.GetType().AssemblyQualifiedName,
            viewEnvelope.View.GetType().Name,
            JsonConvert.SerializeObject(viewEnvelope.MetaData, JsonSerializerSettings),
            viewEnvelope.GlobalVersion.Value,
            viewEnvelope.TenantId ?? "",
            viewEnvelope.CreatedAt ?? DateTime.MinValue)
            {
            }

        public ViewEnvelope ToViewEnvelope() => new(
            Id,
            (JsonConvert.DeserializeObject(View, Type.GetType(ViewType)!) as IView)!, 
            Abstractions.GlobalVersion.Of(GlobalVersion),
            JsonConvert.DeserializeObject<MetaData>(Metadata),
            TenantId == "" ? null : TenantId,
            CreatedAt == DateTime.MinValue ? null : CreatedAt);
    }
}