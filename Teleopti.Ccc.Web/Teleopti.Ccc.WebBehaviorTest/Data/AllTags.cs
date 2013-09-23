using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public class AllTags : Dictionary<string, string>
	{
		public AllTags()
		{
			Add("ConnectionString", IniFileInfo.ConnectionString);
			Add("ConnectionStringMatrix", IniFileInfo.ConnectionStringMatrix);
			Add("ConnectionStringAnalytics", IniFileInfo.ConnectionStringMatrix);
			Add("AnalyticsDb", new SqlConnectionStringBuilder(IniFileInfo.ConnectionStringMatrix).InitialCatalog);
			Add("AnalyticsDatabase", new SqlConnectionStringBuilder(IniFileInfo.ConnectionStringMatrix).InitialCatalog);
			Add("ServerName", IniFileInfo.ServerName);
			Add("Database", IniFileInfo.Database);
			Add("Url", TestSiteConfigurationSetup.Url.ToString());
			Add("Port", TestSiteConfigurationSetup.Port.ToString());
			Add("AgentPortalWebURL", TestSiteConfigurationSetup.Url.ToString());
			Add("SitePath", Paths.WebPath());
			Add("ConfigPath", Paths.WebBinPath());
			Add("MessageBroker", TestSiteConfigurationSetup.Url.ToString());
            Add("MachineKey", CryptoCreator.MachineKeyCreator.GetConfig());
	}

		public AllTags(IEnumerable<KeyValuePair<string, string>> additionalTags)
			: this()
		{
			additionalTags.ForEach(a => Add(a.Key, a.Value));
		}

	}
}