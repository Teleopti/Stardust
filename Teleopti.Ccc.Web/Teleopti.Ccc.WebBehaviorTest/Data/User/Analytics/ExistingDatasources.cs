using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class ExistingDatasources : IAnalyticsDataSetup, IDatasourceData
	{
		private readonly ITimeZoneData _timeZones;

		public int RaptorDefaultDatasourceId { get; set; }
		public DataTable Table { get; set; }

		public ExistingDatasources(ITimeZoneData timeZones) {
			_timeZones = timeZones;
		}

		public void Apply(SqlConnection connection, CultureInfo analyticsDataCulture)
		{
			UpdateRaptorDefaultDatasourceWithTimeZoneId();
			LoadDatasourcesFromDatabase();
		}

		const string raptorDefaultDatasourceName = "TeleoptiCCC";
		const int raptorDefaultDatasourceId = 1;

		private void LoadDatasourcesFromDatabase()
		{
			var command = new SqlCommand("select * from mart.sys_datasource");
			var reader = command.ExecuteReader();
			Table = sys_datasource.CreateTable();
			Table.Load(reader);
			RaptorDefaultDatasourceId = Table.FindDatasourceIdByName(raptorDefaultDatasourceName);
		}

		private void UpdateRaptorDefaultDatasourceWithTimeZoneId()
		{
			var sql = string.Format("update mart.sys_datasource set time_zone_id = {0} where datasource_id = {1}", _timeZones.CetTimeZoneId, raptorDefaultDatasourceId);
			var updateTimeZone = new SqlCommand(sql);
			if (updateTimeZone.ExecuteNonQuery() != 1)
				throw new Exception("Expected 1 rows affected!");
		}
	}
}