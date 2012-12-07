using System.Configuration;
using Teleopti.Ccc.Rta.Interfaces;

namespace Teleopti.Ccc.Rta.Server
{
	public class DatabaseConnectionStringHandler : IDatabaseConnectionStringHandler
	{
		public string AppConnectionString()
		{
			return ConfigurationManager.AppSettings["AppDb"];
		}

		public string DataStoreConnectionString()
		{
			return ConfigurationManager.AppSettings["DataStore"];
		}
	}
}