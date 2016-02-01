using System;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver.CoypuImpl;

namespace Teleopti.Ccc.Scheduling.PerformanceTest
{
	public class FullSchedulingTest
	{
		[Test]
		[Category("ScheduleOptimization")]
		public void MeasurePerformance()
		{
			using (var browserActivator = new CoypuChromeActivator())
			{
				browserActivator.Start(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(500));
				var browserInteractions = browserActivator.GetInteractions();

				logon(browserInteractions, AppConfigs.BusinessUnitName, AppConfigs.UserName, AppConfigs.Password);

				scheduleAndOptimize(browserInteractions, AppConfigs.PlanningPeriodId);

				using (new TimeoutScope(browserActivator, TimeSpan.FromDays(1)))
				{
					browserInteractions.AssertExists(".test-scheduling-result, .test-errorMessage, .server-busy, #Login-container, .test-alert");

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
		}

		private static void scheduleAndOptimize(IBrowserInteractions browserInteractions, string planningPeriodId)
		{
			browserInteractions.GoTo(string.Concat(TestSiteConfigurationSetup.URL, "wfm/#/resourceplanner/planningperiod/", planningPeriodId));
			browserInteractions.Click(".test-schedule-button:enabled");
			browserInteractions.AssertExists(".test-schedule-is-running");
		}

		private static void logon(IBrowserInteractions browserInteractions, string businessUnitName, string userName, string password)
		{
			browserInteractions.GoTo(string.Concat(
				TestSiteConfigurationSetup.URL,
				"Test/Logon",
				string.Format("?businessUnitName={0}&userName={1}&password={2}", businessUnitName, userName, password)));
		}
	}
}