using System.Data.Common;
using Npgsql;
using ViewStore.Abstractions;

namespace ViewStore.Postgres
{
    internal static class Extensions
    {
        public static ViewEnvelopeInternal ToInternal(this ViewEnvelope viewEnvelope) => new(viewEnvelope);

        public static DbBatchCommand ToBatchCommandOn(this string viewId, string tablePath)
        {
            var sql = @$"DELETE FROM {tablePath} WHERE id = @viewId";

            var batchCommand = new NpgsqlBatchCommand(sql);
            batchCommand.Parameters.AddWithValue("viewId", viewId);

            return batchCommand;
        }
    }
}