using System;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
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
        
        public NpgsqlBatchCommand ToBatchCommandOn(string tablePath)
        {
            var sql = @$"
                INSERT INTO {tablePath}
                (
                    id,
                    view,
                    viewType,
                    metadata,
                    globalVersion,
                    lastChangeTimeStamp
                ) 
                VALUES
                (
                    @viewId,
                    @view,
                    @viewType,
                    @metadata,
                    @globalVersion,
                    @lastChangeTimeStamp
                ) 
                ON CONFLICT (id) DO UPDATE
                SET 
                    view = @view,
                    viewType = @viewType,
                    metadata = @metadata,
                    globalVersion = @globalVersion,
                    lastChangeTimeStamp = @lastChangeTimeStamp";

            var batchCommand = new NpgsqlBatchCommand(sql);
            batchCommand.Parameters.AddWithValue("viewId", Id);
            batchCommand.Parameters.AddWithValue("view", NpgsqlDbType.Jsonb, View);
            batchCommand.Parameters.AddWithValue("viewType", ViewType);
            batchCommand.Parameters.AddWithValue("metadata", NpgsqlDbType.Jsonb, Metadata);
            batchCommand.Parameters.AddWithValue("globalVersion", GlobalVersion);
            batchCommand.Parameters.AddWithValue("lastChangeTimeStamp", DateTime.UtcNow);

            return batchCommand;
        }
    }
}