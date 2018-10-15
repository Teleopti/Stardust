using System.Data.Common;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling
{
	public static class DbConnectionExtensions
	{
		public static SqlConnection Unwrap(this DbConnection conn)
		{
			if (conn is ResilientSqlDbConnection connection)
			{
				return connection.ReliableConnection.Current;
			}

			return (SqlConnection) conn;
		}
	}
}