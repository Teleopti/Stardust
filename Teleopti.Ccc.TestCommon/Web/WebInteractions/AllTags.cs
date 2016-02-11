using System.Collections.Generic;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions
{
	public class AllTags : Dictionary<string, string>
	{
		public AllTags()
		{
			// behavior test
			Add("TOGGLEMODE", InfraTestConfigReader.TOGGLE_MODE);
			Add("MachineKey", CryptoCreator.MachineKeyCreator.StaticMachineKeyForBehaviorTest());
			Add("TimeLoggerConfiguration", "<logger name='Teleopti.LogTime'><level value='DEBUG'/></logger>");

			// iisexpress
			Add("Port", TestSiteConfigurationSetup.Port.ToString());
			Add("PortAuthenticationBridge", TestSiteConfigurationSetup.PortAuthenticationBridge.ToString());
			Add("PortWindowsIdentityProvider", TestSiteConfigurationSetup.PortWindowsIdentityProvider.ToString());
			Add("SitePath", Paths.WebPath());
			Add("SitePathAuthenticationBridge", Paths.WebAuthenticationBridgePath());
			Add("SitePathWindowsIdentityProvider", Paths.WebWindowsIdentityProviderPath());

			// settings.txt
			Add("SQL_AUTH_STRING", InfraTestConfigReader.SQL_AUTH_STRING);
			Add("DB_ANALYTICS", InfraTestConfigReader.DB_ANALYTICS);
			Add("SQL_SERVER_NAME", InfraTestConfigReader.SQL_SERVER_NAME);
			Add("WEB_BROKER_BACKPLANE", InfraTestConfigReader.WEB_BROKER_BACKPLANE);
			Add("DB_CCC7", InfraTestConfigReader.DB_CCC7);
			Add("URL", TestSiteConfigurationSetup.URL.ToString());
			Add("UrlAuthenticationBridge", TestSiteConfigurationSetup.UrlAuthenticationBridge.ToString());
			Add("WEB_BROKER_FOR_WEB", TestSiteConfigurationSetup.URL.ToString());
			Add("DEFAULT_IDENTITY_PROVIDER", "Teleopti");
			Add("WEB_DEPLOY", bool.FalseString.ToLowerInvariant());
			Add("WindowsClaimProvider", TestSiteConfigurationSetup.WindowsClaimProvider);
			Add("TeleoptiClaimProvider", TestSiteConfigurationSetup.TeleoptiClaimProvider);
			Add("MATRIX_WEB_SITE_URL", "http://localhost:52510");
		}
	}
}