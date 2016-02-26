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
			tags.Set("MachineKey", CryptoCreator.MachineKeyCreator.StaticMachineKeyForBehaviorTest());
			tags.Set("TimeLoggerConfiguration", "<logger name='Teleopti.LogTime'><level value='DEBUG'/></logger>");
			tags.Set("BehaviorTestServer", "true");
			tags.Set("HangfireDashboard", "true");
			tags.Set("HangfireDashboardStatistics", "true");
			tags.Set("HangfireDashboardCounters", "true");
			tags.Set("HangfireDashboardDisplayNames", "true");
			tags.Set("HangfireJobExpirationSeconds", TimeSpan.FromDays(1).TotalSeconds.ToString());

			// iisexpress
			tags.Set("Port", TestSiteConfigurationSetup.Port.ToString());
			tags.Set("PortAuthenticationBridge", TestSiteConfigurationSetup.PortAuthenticationBridge.ToString());
			tags.Set("PortWindowsIdentityProvider", TestSiteConfigurationSetup.PortWindowsIdentityProvider.ToString());
			tags.Set("SitePath", Paths.WebPath());
			tags.Set("SitePathAuthenticationBridge", Paths.WebAuthenticationBridgePath());
			tags.Set("SitePathWindowsIdentityProvider", Paths.WebWindowsIdentityProviderPath());

			// settings.txt
			tags.Set("URL", TestSiteConfigurationSetup.URL.ToString());
			tags.Set("UrlAuthenticationBridge", TestSiteConfigurationSetup.UrlAuthenticationBridge.ToString());
			tags.Set("WEB_BROKER_FOR_WEB", TestSiteConfigurationSetup.URL.ToString());
			tags.Set("WindowsClaimProvider", TestSiteConfigurationSetup.WindowsClaimProvider);
			tags.Set("TeleoptiClaimProvider", TestSiteConfigurationSetup.TeleoptiClaimProvider);

			return tags;
		}
	}
}