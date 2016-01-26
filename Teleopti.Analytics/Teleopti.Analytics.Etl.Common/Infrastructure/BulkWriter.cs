using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
    public class BulkWriter
    {
		private static readonly ILog logger = LogManager.GetLogger(typeof(BulkWriter));
		const int maxRetry = 5;
		const int delayMs = 100;


		private RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy> makeRetryPolicy()
		{
			var fromMilliseconds = TimeSpan.FromMilliseconds(delayMs);
			var policy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(maxRetry, fromMilliseconds);
			policy.Retrying += (sender, args) =>
			{
				// Log details of the retry.
				var msg = String.Format("Retry - Count:{0}, Delay:{1}, Exception:{2}", args.CurrentRetryCount, args.Delay, args.LastException);
				Trace.WriteLine(msg);
			};
			return policy;
		}

		public void WriteWithRetries(DataTable dataTable, String connectionString, String tableName)
		{
			tryWrite(dataTable, connectionString, tableName);
		}

		private void tryWrite(DataTable dataTable, string connectionString, string tableName)
		{
			var policy = makeRetryPolicy();
			try
			{
				policy.ExecuteAction(() => write(dataTable, connectionString, tableName));
			}
			catch (Exception ex)
			{
				logger.Error("Get exception when executing SqlBulkCopy", ex);
				throw;
			}
		}

		private void write(DataTable dataTable, String connectionString, String tableName)
        {
            using (var destinationConnection = new SqlConnection(connectionString))
            {
                destinationConnection.Open();
            /*
                Set up the bulk copy object. 
                The column positions in the source data reader 
                match the column positions in the destination table, 
                so there is no need to map columns.
             */
                using (var sqlBulkCopy = new SqlBulkCopy(destinationConnection))
                {
                    DateTime startTime = DateTime.Now;
                    sqlBulkCopy.DestinationTableName = tableName;
                    //_sqlBulkCopy.NotifyAfter = 10000;
                    var timeout = 60;
                    if (ConfigurationManager.AppSettings["databaseTimeout"] != null)
                        timeout = int.Parse(ConfigurationManager.AppSettings["databaseTimeout"], CultureInfo.InvariantCulture);
                    sqlBulkCopy.BulkCopyTimeout = timeout;
                    // Write from the source to the destination.
                    sqlBulkCopy.WriteToServer(dataTable);
                    DateTime endTime = DateTime.Now;
                    double duration = endTime.Subtract(startTime).TotalMilliseconds/1000;
                    Trace.WriteLine(string.Concat("Bulk-insert duration: ", duration.ToString("0.00", CultureInfo.CurrentCulture)));
                }
            }
        }
    }
}
