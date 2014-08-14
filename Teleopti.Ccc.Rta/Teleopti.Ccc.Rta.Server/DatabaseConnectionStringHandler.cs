using System.Configuration;
using Teleopti.Ccc.Rta.Interfaces;

namespace Teleopti.Ccc.Rta.Server
{
	public class DatabaseConnectionStringHandler : IDatabaseConnectionStringHandler
	{
		public string AppConnectionString()
		{
			return ConfigurationManager.ConnectionStrings["RtaApplication"] != null
				? ConfigurationManager.ConnectionStrings["RtaApplication"].ConnectionString
				: "";
		}

		public string DataStoreConnectionString()
		{
			return ConfigurationManager.ConnectionStrings["RtaAnalytics"] != null
				? ConfigurationManager.ConnectionStrings["RtaAnalytics"].ConnectionString
				: "";
		}
	}
}