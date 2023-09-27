using System.Data.Common;
using Npgsql;
using NpgsqlTypes;

namespace ViewStore.Postgres;

internal static class Extensions
{
    public static ViewRecordInternal ToInternal(this ViewRecord viewEnvelope) => new(viewEnvelope);
    
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
                    globalVersion
                from {tablePath}
                    where id = @viewId";
            
        var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("viewId", viewId);

        return cmd;
    }
        
    public static NpgsqlBatchCommand ToBatchCommandOn(this ViewRecordInternal viewRecordInternal, string tablePath)
    {
        var sql = @$"
                INSERT INTO {tablePath}
                (
                    id,
                    view,
                    viewType,
                    shortViewType,
                    globalVersion,
                    lastChangeTimeStamp
                ) 
                VALUES
                (
                    @viewId,
                    @view,
                    @viewType,
                    @shortViewType,
                    @globalVersion,
                    @lastChangeTimeStamp
                ) 
                ON CONFLICT (id) DO UPDATE
                SET 
                    view = @view,
                    viewType = @viewType,
                    shortViewType = @shortViewType,
                    globalVersion = @globalVersion,
                    lastChangeTimeStamp = @lastChangeTimeStamp;";

        var batchCommand = new NpgsqlBatchCommand(sql);
        batchCommand.Parameters.AddWithValue("viewId", viewRecordInternal.Id);
        batchCommand.Parameters.AddWithValue("view", NpgsqlDbType.Jsonb, viewRecordInternal.View);
        batchCommand.Parameters.AddWithValue("viewType", viewRecordInternal.ViewType);
        batchCommand.Parameters.AddWithValue("shortViewType", viewRecordInternal.ShortViewType);
        batchCommand.Parameters.AddWithValue("globalVersion", viewRecordInternal.GlobalVersion);
        batchCommand.Parameters.AddWithValue("lastChangeTimeStamp", DateTime.UtcNow);

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