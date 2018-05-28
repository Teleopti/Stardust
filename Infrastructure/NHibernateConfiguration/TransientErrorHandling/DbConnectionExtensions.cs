using System.Data.Common;
using System.Data.SqlClient;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling
{
	public static class DbConnectionExtensions
	{
		public static SqlConnection Unwrap(this DbConnection conn)
		{
			if (conn is ReliableSqlDbConnection dbConnection)
			{
				return dbConnection.ReliableConnection.Current;
			}
			if (conn is ResilientSqlDbConnection connection)
			{
				return connection.ReliableConnection.Current;
			}

			return (SqlConnection) conn;
		}
	}
}