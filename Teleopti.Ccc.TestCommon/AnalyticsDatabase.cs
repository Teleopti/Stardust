using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;

namespace Teleopti.Ccc.TestCommon
{
	public class AnalyticsDatabase
	{
		private readonly IConnectionStrings _connectionStrings;

		public int CurrentDataSourceId;

		public AnalyticsDatabase(IConnectionStrings connectionStrings)
		{
			_connectionStrings = connectionStrings;
		}

		[AnalyticsUnitOfWork]
		public virtual void WithDataSource(int datasourceId, string sourceid)
		{
			CurrentDataSourceId = datasourceId;
			// really, shouldnt make 2 connections!
			using (var connection = new SqlConnection(_connectionStrings.Analytics()))
			{
				connection.Open();
				using (var table = sys_datasource.CreateTable())
				{
					table.AddDataSource(CurrentDataSourceId, " ", -1, " ", -1, " ", " ", 1, false, sourceid, false);
					Bulk.Insert(connection, table);
				}
			}
		}
	}
}