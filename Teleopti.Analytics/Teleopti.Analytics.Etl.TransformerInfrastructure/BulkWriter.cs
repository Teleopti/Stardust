using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
    public static class BulkWriter
    {
        public static void BulkWrite(DataTable dataTable, String connectionString, String tableName)
        {
            using (SqlConnection destinationConnection = new SqlConnection(connectionString))
            {
                destinationConnection.Open();
            /*
                Set up the bulk copy object. 
                The column positions in the source data reader 
                match the column positions in the destination table, 
                so there is no need to map columns.
             */
                using (SqlBulkCopy _sqlBulkCopy = new SqlBulkCopy(destinationConnection))
                {
                    DateTime startTime = DateTime.Now;
                    _sqlBulkCopy.DestinationTableName = tableName;
                    //_sqlBulkCopy.NotifyAfter = 10000;
                    _sqlBulkCopy.BulkCopyTimeout = int.Parse(ConfigurationManager.AppSettings["databaseTimeout"], CultureInfo.InvariantCulture);
                    // Write from the source to the destination.
                    _sqlBulkCopy.WriteToServer(dataTable);
                    DateTime endTime = DateTime.Now;
                    double duration = endTime.Subtract(startTime).TotalMilliseconds/1000;
                    Trace.WriteLine(string.Concat("Bulk-insert duration: ", duration.ToString("0.00", CultureInfo.CurrentCulture)));
                }
            }
        }
    }
}
