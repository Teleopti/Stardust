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

			for (var i = 0; i < 5; i++)
			{
				var result = tryPingManager(managerBaseUrl);
				if (result == true)
				{
					testlog.Debug("StarDustManager ping OK!");
					return;
				}

				testlog.Debug("Failed to ping Stardust Manager. Try pinging again..");
			}
			throw new TimeoutException("Failed to ping Stardust Manager. Aborting test");
		}

		private static bool tryPingManager(string managerBaseUrl)
		{
			var ping = new GetHttpRequest();
			try
			{
				ping.Get<string>(managerBaseUrl + "ping", new NameValueCollection());
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

	}
}
