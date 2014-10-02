using System.Data;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
    public interface IDatabaseConnectionFactory
    {
        IDbConnection CreateConnection(string connectionString);
    }
}