using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class DatabaseConnectionFactory : IDatabaseConnectionFactory
	{
		public SqlConnection CreateConnection(string connectionString)
		{
			return new SqlConnection(connectionString);
		}
	}
}