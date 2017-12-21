using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class DataSource : IAnalyticsDataSetup
	{
		public int DataSourceId { get; set; }
		public string Name { get; set; }
		public int TimeZoneId { get; set; }

		public DataSource()
		{
			DataSourceId = 123;
			Name = "ds name";
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = sys_datasource.CreateTable())
			{
				table.AddDataSource(DataSourceId, Name, -1, Name, -1, "ds db name", "ds type", TimeZoneId, false, "-1", false);

				Bulk.Insert(connection, table);
			}
		}
	}
}