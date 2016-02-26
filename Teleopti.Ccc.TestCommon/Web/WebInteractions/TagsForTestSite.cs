using System;
using Teleopti.Support.Library.Config;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions
{
	public class TagsForTestSite : Tags
	{
		public TagsForTestSite()
		{
			// behavior test
			AddTag("ToggleMode", InfraTestConfigReader.TOGGLE_MODE);
			AddTag("MachineKey", CryptoCreator.MachineKeyCreator.StaticMachineKeyForBehaviorTest());
			AddTag("TimeLoggerConfiguration", "<logger name='Teleopti.LogTime'><level value='DEBUG'/></logger>");
			AddTag("BehaviorTestServer", "true");
			AddTag("HangfireDashboard", "true");
			AddTag("HangfireDashboardStatistics", "true");
			AddTag("HangfireDashboardCounters", "true");
			AddTag("HangfireDashboardDisplayNames", "true");
			AddTag("HangfireJobExpirationSeconds", TimeSpan.FromDays(1).TotalSeconds.ToString());

			// iisexpress
			AddTag("Port", TestSiteConfigurationSetup.Port.ToString());
			AddTag("PortAuthenticationBridge", TestSiteConfigurationSetup.PortAuthenticationBridge.ToString());
			AddTag("PortWindowsIdentityProvider", TestSiteConfigurationSetup.PortWindowsIdentityProvider.ToString());
			AddTag("SitePath", Paths.WebPath());
			AddTag("SitePathAuthenticationBridge", Paths.WebAuthenticationBridgePath());
			AddTag("SitePathWindowsIdentityProvider", Paths.WebWindowsIdentityProviderPath());

			// settings.txt
			AddTag("SQL_AUTH_STRING", InfraTestConfigReader.SQL_AUTH_STRING);
			AddTag("DB_ANALYTICS", InfraTestConfigReader.DB_ANALYTICS);
			AddTag("SQL_SERVER_NAME", InfraTestConfigReader.SQL_SERVER_NAME);
			AddTag("WEB_BROKER_BACKPLANE", InfraTestConfigReader.WEB_BROKER_BACKPLANE);
			AddTag("DB_CCC7", InfraTestConfigReader.DB_CCC7);
			AddTag("URL", TestSiteConfigurationSetup.URL.ToString());
			AddTag("UrlAuthenticationBridge", TestSiteConfigurationSetup.UrlAuthenticationBridge.ToString());
			AddTag("WEB_BROKER_FOR_WEB", TestSiteConfigurationSetup.URL.ToString());
			AddTag("DEFAULT_IDENTITY_PROVIDER", "Teleopti");
			AddTag("WEB_DEPLOY", bool.FalseString.ToLowerInvariant());
			AddTag("USE_PERSISTENT_CRYPTOKEYS", bool.FalseString.ToLowerInvariant());
			AddTag("WindowsClaimProvider", TestSiteConfigurationSetup.WindowsClaimProvider);
			AddTag("TeleoptiClaimProvider", TestSiteConfigurationSetup.TeleoptiClaimProvider);
			AddTag("MATRIX_WEB_SITE_URL", "http://localhost:52510");
		}
	}
}