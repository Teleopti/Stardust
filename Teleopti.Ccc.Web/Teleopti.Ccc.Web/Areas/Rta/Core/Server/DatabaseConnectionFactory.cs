using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        public IDbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}
