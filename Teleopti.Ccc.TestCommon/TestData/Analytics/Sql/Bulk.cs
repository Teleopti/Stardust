using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using NUnit.Framework;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Sql
{
	public static class Bulk
	{
		public static void Insert(SqlConnection connection, DataTable table)
		{
			using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.CheckConstraints, null))
			{
				try
				{
					bulk.BulkCopyTimeout = 60;
					bulk.DestinationTableName = table.TableName;
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