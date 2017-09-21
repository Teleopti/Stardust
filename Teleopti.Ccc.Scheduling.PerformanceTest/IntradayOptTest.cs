using System;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver.CoypuImpl;

namespace Teleopti.Ccc.Scheduling.PerformanceTest
{
	public class IntradayOptTest
	{
		[Test, Ignore("due to commit 6d2375b43c30 removed temp.html")]
		[Category("IntradayOptimization")]
		public void MeasurePerformance()
		{
			using (var browserActivator = new CoypuChromeActivator())
			{
				browserActivator.Start(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(500));
				var browserInteractions = browserActivator.GetInteractions();

				WebAction.Logon(browserInteractions, AppConfigs.BusinessUnitName, AppConfigs.UserName, AppConfigs.Password);

				doIntraday(browserInteractions, AppConfigs.PlanningPeriodId);

				using (new TimeoutScope(browserActivator, TimeSpan.FromDays(1)))
				{
					browserInteractions.AssertExists(".test-is-done, .test-was-failing");
				}

				browserInteractions.AssertNotExists("body", ".test-was-failing");
				browserInteractions.AssertExists(".test-is-done");
			}
		}

		private static void doIntraday(IBrowserInteractions browserInteractions, string planningPeriodId)
		{
			browserInteractions.GoTo(string.Concat(TestSiteConfigurationSetup.URL, "wfm/#/resourceplanner/optimize/", planningPeriodId));
			browserInteractions.Click(".test-btn:enabled");
			browserInteractions.AssertExists(".test-is-running");
		}
	}
}