﻿using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public class AllTags : Dictionary<string, string>
	{
		public AllTags()
		{
			Add("SQL_AUTH_STRING", IniFileInfo.SQL_AUTH_STRING);
			Add("WEB_BROKER_BACKPLANE", IniFileInfo.WEB_BROKER_BACKPLANE);
			Add("ConnectionString", IniFileInfo.ConnectionString);
			Add("ConnectionStringMatrix", IniFileInfo.ConnectionStringMatrix);
			Add("ConnectionStringAnalytics", IniFileInfo.ConnectionStringMatrix);
			Add("DB_ANALYTICS", IniFileInfo.DB_ANALYTICS);
			Add("SQL_SERVER_NAME", IniFileInfo.SQL_SERVER_NAME);
			Add("DB_CCC7", IniFileInfo.DB_CCC7);
			Add("AnalyticsDb", new SqlConnectionStringBuilder(IniFileInfo.ConnectionStringMatrix).InitialCatalog);
			Add("AnalyticsDatabase", new SqlConnectionStringBuilder(IniFileInfo.ConnectionStringMatrix).InitialCatalog);
			Add("Url", TestSiteConfigurationSetup.Url.ToString());
			Add("Port", TestSiteConfigurationSetup.Port.ToString());
			Add("AgentPortalWebURL", TestSiteConfigurationSetup.Url.ToString());
			Add("SitePath", Paths.WebPath());
			Add("ConfigPath", Paths.WebBinPath());
			Add("WEB_BROKER_FOR_WEB", TestSiteConfigurationSetup.Url.ToString());
            Add("MachineKey", CryptoCreator.MachineKeyCreator.GetConfig());
			Add("AGENTPORTALWEB_nhibConfPath", IniFileInfo.AGENTPORTALWEB_nhibConfPath);
	}

		public AllTags(IEnumerable<KeyValuePair<string, string>> additionalTags)
			: this()
		{
			additionalTags.ForEach(a => Add(a.Key, a.Value));
		}

	}
}