using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public interface ITennantDatabaseConnectionFactory
	{
		SqlConnection CreateConnection();
	}
}