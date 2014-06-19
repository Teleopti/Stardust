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

		public static void AddDataSource(
			this DataTable dataTable,
			int dataSourceId,
			string name,
			int logObjectId,
			string logObjectName,
			int dataSourceDatabaseId,
			string dataSourceDatabaseName,
			string dataSourceTypeName,
			int timeZoneId,
			bool inactive,
			string sourceId,
			bool @internal)
		{
			var row = dataTable.NewRow();

			row["datasource_id"] = dataSourceId;
			row["datasource_name"] = name;
			row["log_object_id"] = logObjectId;
			row["log_object_name"] = logObjectName;
			row["datasource_database_id"] = dataSourceDatabaseId;
			row["datasource_database_name"] = dataSourceDatabaseName;
			row["datasource_type_name"] = dataSourceTypeName;
			row["time_zone_id"] = timeZoneId;
			row["inactive"] = inactive;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			row["source_id"] = sourceId;
			row["internal"] = @internal;
			

			dataTable.Rows.Add(row);
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