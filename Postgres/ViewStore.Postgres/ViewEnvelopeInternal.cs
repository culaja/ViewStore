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
        public string Metadata { get; }
        public long GlobalVersion { get; }

        public ViewEnvelopeInternal(
            string id,
            string view,
            string viewType,
            string metadata,
            long globalVersion)
        {
            Id = id;
            View = view;
            ViewType = viewType;
            Metadata = metadata;
            GlobalVersion = globalVersion;
        }

        public ViewEnvelopeInternal(ViewEnvelope viewEnvelope) : this(
            viewEnvelope.Id,
            JsonConvert.SerializeObject(viewEnvelope.View, JsonSerializerSettings),
            viewEnvelope.View.GetType().AssemblyQualifiedName,
            JsonConvert.SerializeObject(viewEnvelope.MetaData, JsonSerializerSettings),
            viewEnvelope.GlobalVersion.Value)
            {
            }

        public ViewEnvelope ToViewEnvelope() => new(
            Id,
            (JsonConvert.DeserializeObject(View, Type.GetType(ViewType)!) as IView)!, 
            Abstractions.GlobalVersion.Of(GlobalVersion),
            JsonConvert.DeserializeObject<MetaData>(Metadata));
    }
}