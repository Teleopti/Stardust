using System.Configuration;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class MultiTenancyDatabaseConnectionStringHandler : IDatabaseConnectionStringHandler
	{
		public string AppConnectionString(string dataSource)
		{
			return "";
		}

		public string DataStoreConnectionString(string dataSource)
		{
			return "";
		}
	}
}