using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.Rta
{
    public interface IDatabaseConnectionFactory
    {
        SqlConnection CreateConnection(string connectionString);
    }
}