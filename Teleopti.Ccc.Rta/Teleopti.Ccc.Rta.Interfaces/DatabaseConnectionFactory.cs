using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Rta.Interfaces
{
    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        public IDbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}
