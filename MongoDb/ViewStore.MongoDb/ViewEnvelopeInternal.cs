using System;
using ViewStore.Abstractions;

namespace ViewStore.MongoDb
{
    internal sealed class ViewEnvelopeInternal
    {
        public string Id { get; }
        public IView View { get; }
        public long GlobalVersion { get; }
        public MetaData MetaData { get; }
        public string? TenantId { get; }
        public DateTime? CreatedAt { get; }

        public ViewEnvelopeInternal(
            string id,
            IView view,
            long globalVersion,
            MetaData metaData,
            string? tenantId,
            DateTime? createdAt)
        {
            Id = id;
            View = view;
            GlobalVersion = globalVersion;
            MetaData = metaData;
            TenantId = tenantId;
            CreatedAt = createdAt;
        }

        public ViewEnvelope ToViewEnvelope() => new(
            Id,
            View,
            Abstractions.GlobalVersion.Of(GlobalVersion),
            MetaData,
            TenantId,
            CreatedAt);

        public static implicit operator ViewEnvelope?(ViewEnvelopeInternal? @internal) =>
            @internal?.ToViewEnvelope();
    }
}