using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using log4net;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class IndexMaintenanceRepository : IIndexMaintenanceRepository
	{
		private readonly IConnectionStrings _connectionStrings;
		private static readonly ILog logger = LogManager.GetLogger(typeof(IndexMaintenanceRepository));
		private TimeSpan betweenRetries;
		public int Retries { get; private set; }
		public SqlException LastSqlException { get; private set; }

		public IndexMaintenanceRepository(IConnectionStrings connectionStrings)
		{
			_connectionStrings = connectionStrings;
			betweenRetries = TimeSpan.FromMinutes(1);
		}

		public void SetTimespanBetweenRetries(TimeSpan span)
		{
			betweenRetries = span;
		}

		public void PerformIndexMaintenance(DatabaseEnum database)
		{
			logger.Debug($"Performing index maintenance for {database}");
			var connectionString = getConnectionString(database);
			var retries = 0;
			bool sqlError;
			do
			{
				try
				{
					sqlError = false;
					HelperFunctions.ExecuteNonQueryMaintenance(CommandType.StoredProcedure, "dbo.IndexMaintenance", connectionString);
				}
				catch (SqlException exception)
				{
					sqlError = true;
					LastSqlException = exception;
					logger.Debug($"Exception when running indexing for database {database}.", exception);
					retries++;
					Retries = retries;
					Thread.Sleep(betweenRetries);
				}
			} while (sqlError && retries < 2);

			if (sqlError && LastSqlException != null)
			{
				throw LastSqlException;
			}
		}

		private string getConnectionString(DatabaseEnum database)
		{
			string connectionString;
			switch (database)
			{
				case DatabaseEnum.Analytics:
					connectionString = _connectionStrings.Analytics();
					break;
				case DatabaseEnum.Application:
					connectionString = _connectionStrings.Application();
					break;
				case DatabaseEnum.Agg:
					connectionString = GetAggConnectionString(getAggName(), _connectionStrings.Analytics());
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(database), database, null);
			}

			return connectionString;
		}

		public static string GetAggConnectionString(string aggName, string analyticsConnectionString)
		{
			const string initCatString = "Initial Catalog=";
			var firstIndex = analyticsConnectionString.IndexOf(initCatString, StringComparison.Ordinal) + initCatString.Length;
			var lastIndex = analyticsConnectionString.IndexOf(";", firstIndex, StringComparison.Ordinal);
			lastIndex = lastIndex == -1 ? analyticsConnectionString.Length : lastIndex;
			return $"{analyticsConnectionString.Substring(0, firstIndex)}{aggName}{analyticsConnectionString.Substring(lastIndex)}";
		}

		private string getAggName()
		{
			var dataTable = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure,
				"mart.etl_job_get_aggdatabase", null,
				_connectionStrings.Analytics()).Tables[0];
			return dataTable.Rows[0]["target_customName"].ToString();
		}
	}
}