using System;
using System.Data.SqlClient;

namespace Stardust.Manager.Context
{
	public class SqlContext : IDisposable
	{
		public SqlContext(string connectionstring)
		{
			if (string.IsNullOrEmpty(connectionstring))
			{
				
			}

			Connectionstring = connectionstring;
		}

		public SqlConnection CreateSqlConnection()
		{
			return new SqlConnection(Connectionstring);
		}


		public string Connectionstring { get; private set; }

		public void Dispose()
		{
		}
	}
}