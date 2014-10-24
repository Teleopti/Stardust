using System.Data;

namespace Teleopti.Ccc.Infrastructure.Rta
{
    public interface IDatabaseConnectionFactory
    {
        IDbConnection CreateConnection(string connectionString);
    }
}