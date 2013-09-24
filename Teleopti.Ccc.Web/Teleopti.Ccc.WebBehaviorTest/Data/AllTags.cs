using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
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
			Add("Url", IniFileInfo.Url);
			Add("WEB_BROKER", IniFileInfo.WEB_BROKER);
			Add("AGENTPORTALWEB_nhibConfPath", IniFileInfo.AGENTPORTALWEB_nhibConfPath);
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