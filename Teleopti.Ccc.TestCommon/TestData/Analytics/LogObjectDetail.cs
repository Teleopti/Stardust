using System;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class LogObjectDetail : IAnalyticsDataSetup
	{
		private readonly DateTime _statisticsUpUntilDate;
		private readonly int _statisticsUpUntilInterval;
		private readonly bool _clearDataInsteadOfInsert;

		public LogObjectDetail(DateTime statisticsUpUntilDate, int statisticsUpUntilInterval, bool clearDataInsteadOfInsert = false)
		{
			_statisticsUpUntilDate = statisticsUpUntilDate;
			_statisticsUpUntilInterval = statisticsUpUntilInterval;
			_clearDataInsteadOfInsert = clearDataInsteadOfInsert;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			if (_clearDataInsteadOfInsert)
			{
				var sql = $"delete from dbo.log_object_detail where log_object_id = {-1} and detail_id = {2}";
				using (var deleteCommand = new SqlCommand(sql, connection))
				{
					deleteCommand.ExecuteNonQuery();
				}

				sql = $"delete from dbo.log_object where log_object_id = {-1}";
				using (var deleteCommand = new SqlCommand(sql, connection))
				{
					deleteCommand.ExecuteNonQuery();
				}

				sql = $"delete from dbo.acd_type where acd_type_id = {1}";
				using (var deleteCommand = new SqlCommand(sql, connection))
				{
					deleteCommand.ExecuteNonQuery();
				}

				return;
			}

			using (var table = acd_type.CreateTable())
			{
				table.AddAcdType(1, "acd type");
				Bulk.Insert(connection, table);
			}

			using (var table = log_object.CreateTable())
			{
				table.AddLogObject(-1, 1, "test log object", "test db name", 96, 60, 1);
				Bulk.Insert(connection, table);
			}

			using (var table = log_object_detail.CreateTable())
			{
				table.AddLogObjectDetail(-1, 2, "Agent log", "sp name", _statisticsUpUntilInterval, _statisticsUpUntilDate);
				Bulk.Insert(connection, table);
			}
		}
	}
}