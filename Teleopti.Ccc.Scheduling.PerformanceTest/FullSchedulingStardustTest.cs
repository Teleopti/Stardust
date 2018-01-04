using System;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver.CoypuImpl;

namespace Teleopti.Ccc.Scheduling.PerformanceTest
{
	public class FullSchedulingStardustTest
	{
		[Test]
		[Category("ScheduleOptimizationStardust")]
		public void MeasurePerformanceOnStardust()
		{
			using (var browserActivator = new CoypuChromeActivator())
			{
				//long timeout for now due to slow loading of planning period view on large dbs. Could be lowered when fixed
				browserActivator.Start(TimeSpan.FromSeconds(90), TimeSpan.FromMilliseconds(500));
				var browserInteractions = browserActivator.GetInteractions();

				WebAction.Logon(browserInteractions, AppConfigs.BusinessUnitName, AppConfigs.UserName, AppConfigs.Password);

				scheduleAndOptimize(browserInteractions, AppConfigs.PlanningGroupId, AppConfigs.PlanningPeriodId);

				using (new TimeoutScope(browserActivator, TimeSpan.FromDays(1)))
				{
					browserInteractions.AssertExistsUsingJQuery(".heatmap:visible, .test-errorMessage, .server-busy, #Login-container, .test-alert, .notice-warning");
				}

				//no server error
				browserInteractions.AssertNotExists("body", ".notice-warning");
				//no server error
				browserInteractions.AssertNotExists("body", ".test-errorMessage");
				//no failing request
				browserInteractions.AssertNotExists("body", ".test-alert");
				//not showing "server busy"
				browserInteractions.AssertNotExists("body", ".server-busy");
				//not redirected to logon page
				browserInteractions.AssertNotExists("body", "#Login-container");
				browserInteractions.AssertExistsUsingJQuery(".heatmap:visible");
			}
		}

		private static void scheduleAndOptimize(IBrowserInteractions browserInteractions, string planningGroupId, string planningPeriodId)
		{
			browserInteractions.GoTo($"{TestSiteConfigurationSetup.URL}wfm/#/resourceplanner/planninggroup/{planningGroupId}/detail/{planningPeriodId}");
			browserInteractions.Click(".schedule-button:enabled");
			browserInteractions.AssertExists(".test-schedule-is-running");
		}
	}
}