using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Sql
{
	public static class Bulk
	{
		public static void Insert(SqlConnection connection, DataTable table)
		{
			using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity, null))
			{
				bulk.BulkCopyTimeout = 60;
				bulk.DestinationTableName = table.TableName;
				bulk.WriteToServer(table);
			}
		}
	}
}