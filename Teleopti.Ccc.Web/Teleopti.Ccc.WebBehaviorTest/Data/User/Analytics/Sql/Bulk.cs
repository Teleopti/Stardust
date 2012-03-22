using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Sql
{
	public static class Bulk
	{
		public static void Insert(SqlConnection connection, DataTable table)
		{
			using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity, null))
			{
				bulk.DestinationTableName = table.TableName;
				bulk.WriteToServer(table);
			}
		}
	}
}