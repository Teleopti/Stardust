namespace Teleopti.Ccc.Infrastructure.Rta
{
	public interface IDatabaseConnectionStringHandler
	{
		string AppConnectionString();
		string DataStoreConnectionString();
	}
}