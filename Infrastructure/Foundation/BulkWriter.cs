using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class BulkWriter
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(BulkWriter));
		private const int maxRetry = 5;
		private const int delayMs = 100;
		private readonly int _bulkTimeoutSeconds;

		public BulkWriter()
		{
			var couldParseTimeout = int.TryParse(ConfigurationManager.AppSettings["databaseTimeout"], out _bulkTimeoutSeconds);
			if (ConfigurationManager.AppSettings["databaseTimeout"] == null || !couldParseTimeout)
			{
				_bulkTimeoutSeconds = 60;
			}
		}

		public void WriteWithRetries(DataTable dataTable, string connectionString, string tableName)
		{
			tryWrite(dataTable, connectionString, tableName);
		}

		private RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy> makeRetryPolicy()
		{
			var fromMilliseconds = TimeSpan.FromMilliseconds(delayMs);
			var policy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(maxRetry, fromMilliseconds);
			policy.Retrying += (sender, args) =>
			{
				logger.Debug($"Retry - Count:{args.CurrentRetryCount}, Delay:{args.Delay}, Exception:{args.LastException}");
			};
			return policy;
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

		private void write(DataTable dataTable, string connectionString, string tableName)
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
					var startTime = DateTime.Now;
					sqlBulkCopy.DestinationTableName = tableName;
					sqlBulkCopy.BulkCopyTimeout = _bulkTimeoutSeconds;

					// Write from the source to the destination.
					sqlBulkCopy.WriteToServer(dataTable);
					var duration = DateTime.Now.Subtract(startTime).TotalMilliseconds / 1000;
					logger.Debug($"Bulk-insert duration: {duration.ToString("0.00", CultureInfo.CurrentCulture)}");
				}
			}
		}
	}
}
