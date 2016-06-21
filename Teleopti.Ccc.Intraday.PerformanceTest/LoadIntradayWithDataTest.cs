using System;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver.CoypuImpl;

namespace Teleopti.Ccc.Intraday.PerformanceTest
{
	[TestFixture]
	[IntradayPerformanceTestAttribute]
	public class LoadIntradayWithDataTest
	{
		public TimeSetter TimeSetter;

		[Test]
		public void MeasurePerformance()
		{
			TimeSetter.SetDateTime("2016-05-27 00:00");

			using (var browserActivator = new CoypuChromeActivator())
			{
				browserActivator.Start(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(500));
				var browserInteractions = browserActivator.GetInteractions();

				WebAction.Logon(browserInteractions, "Telia Sverige", "demo", "demo");

				logonToIntraday(browserInteractions);

				using (new TimeoutScope(browserActivator, TimeSpan.FromSeconds(60)))
				{
					browserInteractions.AssertJavascriptResultContains("return $('.offered-calls').text().length > 0", "True");
				}
				browserInteractions.AssertNotVisibleUsingJQuery(".no-data-available");
			}
		}
		
		private static void logonToIntraday(IBrowserInteractions browserInteractions)
		{
			browserInteractions.GoTo(string.Concat(TestSiteConfigurationSetup.URL, "wfm/#/intraday"));
			browserInteractions.AssertAnyContains(".skill-list-header b", "All skills");
		}
	}
}