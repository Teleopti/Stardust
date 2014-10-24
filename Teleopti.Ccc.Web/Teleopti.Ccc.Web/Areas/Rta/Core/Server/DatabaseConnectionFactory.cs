using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Infrastructure.Rta;

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
