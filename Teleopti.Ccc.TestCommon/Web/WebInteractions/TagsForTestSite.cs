using System;
using Teleopti.Support.Library.Config;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions
{
	public class TagsForTestSite
	{
		public Tags Make()
		{
			var tags = new SettingsFileManager().ReadFile();

			// behavior test
			tags.SetByName("MachineKey", CryptoCreator.MachineKeyCreator.StaticMachineKeyForBehaviorTest());
			tags.SetByName("TimeLoggerConfiguration", "<logger name='Teleopti.LogTime'><level value='DEBUG'/></logger>");
			tags.SetByName("BehaviorTestServer", "true");
			tags.SetByName("HangfireDashboard", "true");
			tags.SetByName("HangfireDashboardStatistics", "true");
			tags.SetByName("HangfireDashboardCounters", "true");
			tags.SetByName("HangfireDashboardDisplayNames", "true");
			tags.SetByName("HangfireJobExpirationSeconds", TimeSpan.FromDays(1).TotalSeconds.ToString());

			// iisexpress
			tags.SetByName("Port", TestSiteConfigurationSetup.Port.ToString());
			tags.SetByName("PortAuthenticationBridge", TestSiteConfigurationSetup.PortAuthenticationBridge.ToString());
			tags.SetByName("PortWindowsIdentityProvider", TestSiteConfigurationSetup.PortWindowsIdentityProvider.ToString());
			tags.SetByName("SitePath", Paths.WebPath());
			tags.SetByName("SitePathAuthenticationBridge", Paths.WebAuthenticationBridgePath());
			tags.SetByName("SitePathWindowsIdentityProvider", Paths.WebWindowsIdentityProviderPath());

			// settings.txt
			tags.SetByName("URL", TestSiteConfigurationSetup.URL.ToString());
			tags.SetByName("UrlAuthenticationBridge", TestSiteConfigurationSetup.UrlAuthenticationBridge.ToString());
			tags.SetByName("WEB_BROKER_FOR_WEB", TestSiteConfigurationSetup.URL.ToString());
			tags.SetByName("WindowsClaimProvider", TestSiteConfigurationSetup.WindowsClaimProvider);
			tags.SetByName("TeleoptiClaimProvider", TestSiteConfigurationSetup.TeleoptiClaimProvider);

			return tags;
		}
	}
}