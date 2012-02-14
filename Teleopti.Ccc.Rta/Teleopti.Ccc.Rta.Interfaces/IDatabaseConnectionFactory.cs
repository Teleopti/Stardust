using System.Data;

namespace Teleopti.Ccc.Rta.Interfaces
{
    public interface IDatabaseConnectionFactory
    {
        IDbConnection CreateConnection(string connectionString);
    }
}