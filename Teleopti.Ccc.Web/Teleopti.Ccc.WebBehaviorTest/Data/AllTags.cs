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

			Add("UrlAuthenticationBridge", TestSiteConfigurationSetup.UrlAuthenticationBridge.ToString());
			Add("PortAuthenticationBridge", TestSiteConfigurationSetup.PortAuthenticationBridge.ToString());

			Add("PortWindowsIdentityProvider", TestSiteConfigurationSetup.PortWindowsIdentityProvider.ToString());

			Add("AgentPortalWebURL", TestSiteConfigurationSetup.Url.ToString());
			Add("SitePath", Paths.WebPath());
			Add("SitePathAuthenticationBridge", Paths.WebAuthenticationBridgePath());
			Add("SitePathWindowsIdentityProvider", Paths.WebWindowsIdentityProviderPath());
			Add("ConfigPath", Paths.WebBinPath());
			Add("WEB_BROKER_FOR_WEB", TestSiteConfigurationSetup.Url.ToString());
			Add("DEFAULT_IDENTITY_PROVIDER", "Teleopti");
			Add("WindowsClaimProvider", TestSiteConfigurationSetup.WindowsClaimProvider);
			Add("TeleoptiClaimProvider", TestSiteConfigurationSetup.TeleoptiClaimProvider);
			Add("MATRIX_WEB_SITE_URL", "http://localhost:52510");
			Add("MachineKey", CryptoCreator.MachineKeyCreator.StaticMachineKeyForBehaviorTest());
			Add("AGENTPORTALWEB_nhibConfPath", IniFileInfo.AGENTPORTALWEB_nhibConfPath);
			Add("TOGGLE_FILE", IniFileInfo.FeatureToggle);
	}

		public AllTags(IEnumerable<KeyValuePair<string, string>> additionalTags)
			: this()
		{
			additionalTags.ForEach(a => Add(a.Key, a.Value));
		}

	}
}