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
			tags.SetVariantsOf("MachineKey", CryptoCreator.MachineKeyCreator.StaticMachineKeyForBehaviorTest());
			tags.SetVariantsOf("TimeLoggerConfiguration", "<logger name='Teleopti.LogTime'><level value='DEBUG'/></logger>");
			tags.SetVariantsOf("BehaviorTestServer", "true");
			tags.SetVariantsOf("HangfireDashboard", "true");
			tags.SetVariantsOf("HangfireDashboardStatistics", "true");
			tags.SetVariantsOf("HangfireDashboardCounters", "true");
			tags.SetVariantsOf("HangfireDashboardDisplayNames", "true");
			tags.SetVariantsOf("HangfireJobExpirationSeconds", TimeSpan.FromDays(1).TotalSeconds.ToString());

			// iisexpress
			tags.SetVariantsOf("Port", TestSiteConfigurationSetup.Port.ToString());
			tags.SetVariantsOf("PortAuthenticationBridge", TestSiteConfigurationSetup.PortAuthenticationBridge.ToString());
			tags.SetVariantsOf("PortWindowsIdentityProvider", TestSiteConfigurationSetup.PortWindowsIdentityProvider.ToString());
			tags.SetVariantsOf("SitePath", Paths.WebPath());
			tags.SetVariantsOf("SitePathAuthenticationBridge", Paths.WebAuthenticationBridgePath());
			tags.SetVariantsOf("SitePathWindowsIdentityProvider", Paths.WebWindowsIdentityProviderPath());

			// settings.txt
			tags.SetVariantsOf("URL", TestSiteConfigurationSetup.URL.ToString());
			tags.SetVariantsOf("UrlAuthenticationBridge", TestSiteConfigurationSetup.UrlAuthenticationBridge.ToString());
			tags.SetVariantsOf("WEB_BROKER_FOR_WEB", TestSiteConfigurationSetup.URL.ToString());
			tags.SetVariantsOf("WindowsClaimProvider", TestSiteConfigurationSetup.WindowsClaimProvider);
			tags.SetVariantsOf("TeleoptiClaimProvider", TestSiteConfigurationSetup.TeleoptiClaimProvider);

			return tags;
		}
	}
}