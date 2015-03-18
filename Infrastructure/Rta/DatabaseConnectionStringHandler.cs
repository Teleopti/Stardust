using System.Configuration;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class DatabaseConnectionStringHandler : IDatabaseConnectionStringHandler
	{
		public string AppConnectionString(string tenant)
		{
			return ConfigurationManager.ConnectionStrings["RtaApplication"] != null
				? ConfigurationManager.ConnectionStrings["RtaApplication"].ConnectionString
				: "";
		}

		public string DataStoreConnectionString(string tenant)
		{
			return ConfigurationManager.ConnectionStrings["RtaAnalytics"] != null
				? ConfigurationManager.ConnectionStrings["RtaAnalytics"].ConnectionString
				: "";
		}
	}
}