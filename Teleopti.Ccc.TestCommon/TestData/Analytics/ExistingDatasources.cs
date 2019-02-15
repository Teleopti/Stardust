using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class ExistingDatasources : IAnalyticsDataSetup, IDatasourceData
	{
		private readonly ITimeZoneUtcAndCet _timeZones;

		public int RaptorDefaultDatasourceId { get; set; }
		public IEnumerable<DataRow> Rows { get; set; }

		public ExistingDatasources(ITimeZoneUtcAndCet timeZones) {
			_timeZones = timeZones;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			UpdateRaptorDefaultDatasourceWithTimeZoneId(connection);
			LoadDatasourcesFromDatabase(connection);
		}

		const string raptorDefaultDatasourceName = "TeleoptiCCC";
		public static int DefaultRaptorDefaultDatasourceId = 1;

		private void LoadDatasourcesFromDatabase(SqlConnection connection)
		{
			using (var table = sys_datasource.CreateTable())
			{
				using (var command = new SqlCommand("select * from mart.sys_datasource", connection))
				{
					var reader = command.ExecuteReader();
					table.Load(reader);
				}
				Rows = table.AsEnumerable();
			}
			RaptorDefaultDatasourceId = Rows.FindDatasourceIdByName(raptorDefaultDatasourceName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		private void UpdateRaptorDefaultDatasourceWithTimeZoneId(SqlConnection connection)
		{
			var sql = string.Format("update mart.sys_datasource set time_zone_id = {0} where datasource_id = {1}", _timeZones.CetTimeZoneId, DefaultRaptorDefaultDatasourceId);
			using (var updateTimeZone = new SqlCommand(sql, connection))
			{
				if (updateTimeZone.ExecuteNonQuery() != 1)
					throw new Exception("Expected 1 rows affected!");
			}
		}
	}
}