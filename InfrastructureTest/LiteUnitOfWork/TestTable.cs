using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.LiteUnitOfWork
{
	public class TestTable : IDisposable
	{
		private readonly string _name;

		public TestTable(string name)
		{
			_name = name;
			applySql(string.Format("CREATE TABLE {0} (Value int)", _name));
		}

		public static IEnumerable<int> Values(string tableName)
		{
			using (var connection = new SqlConnection(ConnectionStringHelper.ConnectionStringUsedInTests))
			{
				connection.Open();
				using (var command = new SqlCommand("SELECT * FROM " + tableName, connection))
				using (var reader = command.ExecuteReader())
					while (reader.Read())
						yield return reader.GetInt32(0);
			}
		}

		public void Dispose()
		{
			try
			{
				applySql(string.Format("DROP TABLE {0}", _name));
			}
			catch (Exception)
			{
			}
		}

		private static void applySql(string Sql)
		{
			using (var connection = new SqlConnection(ConnectionStringHelper.ConnectionStringUsedInTests))
			{
				connection.Open();
				using (var command = new SqlCommand(Sql, connection))
					command.ExecuteNonQuery();
			}
		}
	}
}