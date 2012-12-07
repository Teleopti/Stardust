using System.Data;

namespace Teleopti.Ccc.Rta.Interfaces
{
    public interface IDatabaseConnectionFactory
    {
        IDbConnection CreateConnection(string connectionString);
    }

	public interface IDatabaseConnectionStringHandler
	{
		string AppConnectionString();
		string DataStoreConnectionString();
	}
}