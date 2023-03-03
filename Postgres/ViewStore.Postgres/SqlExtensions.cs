using System;
using System.Data.Common;
using Npgsql;
using NpgsqlTypes;

namespace ViewStore.Postgres
{
    internal static class SqlExtensions
    {
        public static NpgsqlCommand ToReadLastGlobalVersionCommandOn(this NpgsqlConnection connection, string tablePath)
        {
            var sql = @$"
                select 
                    globalVersion 
                from {tablePath}
                    order by globalVersion desc
                limit 1";
            
            return new NpgsqlCommand(sql, connection);
        }

        public static NpgsqlCommand ToReadViewIdCommandOn(this NpgsqlConnection connection, string viewId, string tablePath)
        {
            var sql = @$"
                select
                    (view)::text,
                    viewType,
                    shortViewType,
                    (metadata)::text,
                    globalVersion,
                    tenantId,
                    createdAt
                from {tablePath}
                    where id = @viewId";
            
            var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("viewId", viewId);

            return cmd;
        }
        
        public static NpgsqlBatchCommand ToBatchCommandOn(this ViewEnvelopeInternal viewEnvelopeInternal, string tablePath)
        {
            var sql = @$"
                INSERT INTO {tablePath}
                (
                    id,
                    view,
                    viewType,
                    shortViewType,
                    metadata,
                    globalVersion,
                    lastChangeTimeStamp,
                    tenantId,
                    createdAt
                ) 
                VALUES
                (
                    @viewId,
                    @view,
                    @viewType,
                    @shortViewType,
                    @metadata,
                    @globalVersion,
                    @lastChangeTimeStamp,
                    @tenantId,
                    @createdAt
                ) 
                ON CONFLICT (id,tenantId,createdAt) DO UPDATE
                SET 
                    view = @view,
                    viewType = @viewType,
                    shortViewType = @shortViewType,
                    metadata = @metadata,
                    globalVersion = @globalVersion,
                    lastChangeTimeStamp = @lastChangeTimeStamp,
                    tenantId = @tenantId,
                    createdAt = @createdAt";

            var batchCommand = new NpgsqlBatchCommand(sql);
            batchCommand.Parameters.AddWithValue("viewId", viewEnvelopeInternal.Id);
            batchCommand.Parameters.AddWithValue("view", NpgsqlDbType.Jsonb, viewEnvelopeInternal.View);
            batchCommand.Parameters.AddWithValue("viewType", viewEnvelopeInternal.ViewType);
            batchCommand.Parameters.AddWithValue("shortViewType", viewEnvelopeInternal.ShortViewType);
            batchCommand.Parameters.AddWithValue("metadata", NpgsqlDbType.Jsonb, viewEnvelopeInternal.Metadata);
            batchCommand.Parameters.AddWithValue("globalVersion", viewEnvelopeInternal.GlobalVersion);
            batchCommand.Parameters.AddWithValue("lastChangeTimeStamp", DateTime.UtcNow);
            batchCommand.Parameters.AddWithValue("tenantId", viewEnvelopeInternal.TenantId);
            batchCommand.Parameters.AddWithValue("createdAt", viewEnvelopeInternal.CreatedAt);

            return batchCommand;
        }


        public static DbBatchCommand ToBatchCommandOn(this string viewId, string tablePath)
        {
            var sql = @$"DELETE FROM {tablePath} WHERE id = @viewId";

            var batchCommand = new NpgsqlBatchCommand(sql);
            batchCommand.Parameters.AddWithValue("viewId", viewId);

            return batchCommand;
        }
    }
}