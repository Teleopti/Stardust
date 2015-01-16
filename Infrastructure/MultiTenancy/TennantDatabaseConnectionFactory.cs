using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class TennantDatabaseConnectionFactory : ITennantDatabaseConnectionFactory
	{
		private readonly string _tennantConnectionString;

		public TennantDatabaseConnectionFactory(string tennantConnectionString)
		{
			_tennantConnectionString = tennantConnectionString;
		}

		public SqlConnection CreateConnection()
		{
			var conn = new SqlConnection(_tennantConnectionString);
			conn.Open();
			return conn;
		}
	}
}