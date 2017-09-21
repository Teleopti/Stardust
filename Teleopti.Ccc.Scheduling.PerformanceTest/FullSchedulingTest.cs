using System;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver.CoypuImpl;

namespace Teleopti.Ccc.Scheduling.PerformanceTest
{
	public class FullSchedulingTest
	{
		[Test, Ignore("due to commit 6d2375b43c30")]
		[Category("ScheduleOptimization")]
		public void MeasurePerformance()
		{
			using (var browserActivator = new CoypuChromeActivator())
			{
				//long timeout for now due to slow loading of planning period view on large dbs. Could be lowered when fixed
				browserActivator.Start(TimeSpan.FromSeconds(90), TimeSpan.FromMilliseconds(500));
				var browserInteractions = browserActivator.GetInteractions();

				WebAction.Logon(browserInteractions, AppConfigs.BusinessUnitName, AppConfigs.UserName, AppConfigs.Password);

				scheduleAndOptimize(browserInteractions, AppConfigs.PlanningPeriodId);

				using (new TimeoutScope(browserActivator, TimeSpan.FromDays(1)))
				{
					browserInteractions.AssertExists(".test-scheduling-result, .test-errorMessage, .server-busy, #Login-container, .test-alert");
				}
				//no server error
				browserInteractions.AssertNotExists("body", ".test-errorMessage");
				//no failing request
				browserInteractions.AssertNotExists("body", ".test-alert");
				//not showing "server busy"
				browserInteractions.AssertNotExists("body", ".server-busy");
				//not redirected to logon page
				browserInteractions.AssertNotExists("body", "#Login-container");
				browserInteractions.AssertExists(".test-scheduling-result");
			}
		}

		private static void scheduleAndOptimize(IBrowserInteractions browserInteractions, string planningPeriodId)
		{
			browserInteractions.GoTo(string.Concat(TestSiteConfigurationSetup.URL, "wfm/#/resourceplanner/planningperiod/", planningPeriodId, "?runForTest=true"));
			browserInteractions.Click(".test-schedule-button:enabled");
			browserInteractions.AssertExists(".test-schedule-is-running");
		}
	}
}