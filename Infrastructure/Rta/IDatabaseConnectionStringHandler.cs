namespace Teleopti.Ccc.Infrastructure.Rta
{
	public interface IDatabaseConnectionStringHandler
	{
		string AppConnectionString(string tenant);
		string DataStoreConnectionString(string tenant);
	}
}