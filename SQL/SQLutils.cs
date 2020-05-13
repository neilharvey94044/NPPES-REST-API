using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace NppesAPI.SQL
{
    public static class SQLutils
    {

        public static void logDebugSchema(SqlDataReader rdr, ILogger logger)
        {
            DataTable schemaTable = rdr.GetSchemaTable();

            foreach (DataRow row in schemaTable.Rows)
            {
                foreach (DataColumn column in schemaTable.Columns)
                {
                    logger.LogDebug(String.Format("{0} = {1}",
                       column.ColumnName, row[column]));
                }
            }
        }

        public static string GetColString(this IDataRecord rcrd, string colname)
        {
            int ord = rcrd.GetOrdinal(colname);
            return rcrd.IsDBNull(ord) ? "" : rcrd.GetString(ord);
        }
    }
}
