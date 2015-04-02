using System.Configuration;

namespace Teleopti.Ccc.Infrastructure.Rta
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