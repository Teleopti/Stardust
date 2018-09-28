using System;
using System.Collections.Specialized;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Wfm.Stardust.IntegrationTest.Stardust
{
	static class StardustManagerPingHelper
	{
		public static void WaitForStarDustManagerToStart(TestLog testlog)
		{
			testlog.Debug("waitForStarDustManagerToStart");
			var managerBaseUrl = TestSiteConfigurationSetup.URL.AbsoluteUri + @"StardustDashboard/";

			var result = tryPingManager(managerBaseUrl);
			if (!result)
			{
				testlog.Debug("Failed to ping Stardust Manager. Try pinging again..");
				throw new TimeoutException("Failed to ping Stardust Manager. Aborting test");
			}
			testlog.Debug("StarDustManager ping OK!");
		}

		private static bool tryPingManager(string managerBaseUrl)
		{
			try
			{
				var ping = new GetHttpRequest();
				ping.Get<string>(managerBaseUrl + "ping", new NameValueCollection());
				return true;
			}
			catch (Exception ex)
			{
				TestLog.Static.Debug(ex.Message);
				return false;
			}
		}

	}
}
