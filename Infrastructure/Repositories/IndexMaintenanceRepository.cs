using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class IndexMaintenanceRepository : IIndexMaintenanceRepository
	{
		private readonly IConnectionStrings _connectionStrings;

		public IndexMaintenanceRepository(IConnectionStrings connectionStrings)
		{
			_connectionStrings = connectionStrings;
		}

		public void PerformIndexMaintenanceForAll()
		{
			performIndexMaintenance("App");
			performIndexMaintenance("Analytics");
			performIndexMaintenance("Agg");
		}

		private string getAggName()
		{
			var dataTable = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure,
				"mart.etl_job_get_aggdatabase", null,
				_connectionStrings.Analytics()).Tables[0];
			return dataTable.Rows[0]["target_customName"].ToString();
		}

		private void performIndexMaintenance(string database)
		{
			string connectionString = null;
			var dataMartConnectionString = _connectionStrings.Analytics();
			switch (database)
			{
				case "Analytics":
					connectionString = dataMartConnectionString;
					break;
				case "App":
					connectionString = _connectionStrings.Application();
					break;
				case "Agg":
				{
					const string initCatString = "Initial Catalog=";

					var firstIndex = dataMartConnectionString.IndexOf(initCatString) + initCatString.Length;
					var lastIndex = dataMartConnectionString.IndexOf(";", firstIndex);

					var aggName = getAggName();

					connectionString = dataMartConnectionString.Substring(0, firstIndex) + aggName +
									   dataMartConnectionString.Substring(lastIndex);
				}
					break;
			}

			var retries = 0;
			bool sqlError;
			do
			{
				try
				{
					sqlError = false;
					HelperFunctions.ExecuteNonQueryMaintenance(CommandType.StoredProcedure, "dbo.IndexMaintenance", connectionString);
				}
				catch (SqlException)
				{
					sqlError = true;
					retries++;
					Thread.Sleep(TimeSpan.FromMinutes(1));
				}
			} while (sqlError && retries < 2);
		}
	}
}