using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class sys_datasource
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.sys_datasource");
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("datasource_name");
			table.Columns.Add("log_object_id", typeof(int));
			table.Columns.Add("log_object_name");
			table.Columns.Add("datasource_database_id", typeof(int));
			table.Columns.Add("datasource_database_name");
			table.Columns.Add("datasource_type_name");
			table.Columns.Add("time_zone_id", typeof(int));
			table.Columns.Add("inactive", typeof(bool));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("source_id");
			table.Columns.Add("internal", typeof(bool));
			return table;
		}

		public static int FindTimeZoneIdByDatasourceId(
			this IEnumerable<DataRow> dataTable,
			int datasource_id)
		{
			return (
			       	from s in dataTable
			       	where (int) s["datasource_id"] == datasource_id
			       	select (int) s["time_zone_id"]
			       ).Single();
		}

		public static int FindDatasourceIdByName(
			this IEnumerable<DataRow> dataTable,
			string datasource_name)
		{
			return (
			       	from s in dataTable
			       	where (string) s["datasource_name"] == datasource_name
			       	select (int) s["datasource_id"]
			       ).Single();
		}
	}
}