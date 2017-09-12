using System.Data;
using System.Data.SqlClient;
using NUnit.Framework;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Sql
{
	public static class Bulk
	{
		public static void Insert(SqlConnection connection, DataTable table)
		{
			using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity, null))
			{
				try
				{
					bulk.BulkCopyTimeout = 60;
					bulk.DestinationTableName = table.TableName;
					if (bulk.DestinationTableName == "mart.dim_time_zone")
					{
						foreach (DataColumn column in table.Columns)
						{
							if (column.ColumnName != "only_one_default_zone")//this is the computed column, skip it
								bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
						}
					}
					
					bulk.WriteToServer(table);
				}
				catch (SqlException exc)
				{
					TestContext.WriteLine(exc.Message);
				}
			}
		}
	}
}