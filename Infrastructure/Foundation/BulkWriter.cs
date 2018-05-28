using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using log4net;
using Polly;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class BulkWriter
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(BulkWriter));
		private const int maxRetry = 7;
		private readonly int _bulkTimeoutSeconds;
		public int Retries { get; private set; }

		public BulkWriter()
		{
			var couldParseTimeout = int.TryParse(ConfigurationManager.AppSettings["databaseTimeout"], out _bulkTimeoutSeconds);
			if (ConfigurationManager.AppSettings["databaseTimeout"] == null || !couldParseTimeout)
			{
				_bulkTimeoutSeconds = 60;
			}
		}

		public BulkWriter(int bulkTimeoutSeconds)
		{
			_bulkTimeoutSeconds = bulkTimeoutSeconds;
		}

		public void WriteWithRetries(DataTable dataTable, string connectionString, string tableName)
		{
			tryWrite(dataTable, connectionString, tableName);
		}

		private Policy makeRetryPolicy()
		{
			var policy = Policy.Handle<TimeoutException>()
				.Or<SqlException>(DetectTransientSqlException.IsTransient)
				.OrInner<SqlException>(DetectTransientSqlException.IsTransient)
				.WaitAndRetry(maxRetry, i => TimeSpan.FromSeconds(Math.Min(60d, Math.Pow(2, i))),
					(exception, span, retries, ctx) =>
					{
						Retries = retries;
						logger.Debug($"Retry - Count:{retries}, Delay:{span}, Exception:{exception}");
					});
			return policy;
		}

		private void tryWrite(DataTable dataTable, string connectionString, string tableName)
		{
			Retries = 0;
			var policy = makeRetryPolicy();
			try
			{
				policy.Execute(() =>
				{
					write(dataTable, connectionString, tableName);
				});
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
