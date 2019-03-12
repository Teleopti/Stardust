using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver.CoypuImpl;

namespace Teleopti.Ccc.Scheduling.PerformanceTest.Domain
{
	public class SchedulingAndDOTest
	{
		[Test]
		[Category("ScheduleOptimizationStardust")]
		public void SchedulingAndDOOpt()
		{
			using (var browserActivator = new CoypuChromeActivator())
			{
				//long timeout for now due to slow loading of planning period view on large dbs. Could be lowered when fixed
				browserActivator.Start(TimeSpan.FromSeconds(90), TimeSpan.FromMilliseconds(500));
				var browserInteractions = browserActivator.GetInteractions();

				WebAction.Logon(browserInteractions, AppConfigs.BusinessUnitName, AppConfigs.UserName, AppConfigs.Password);

				browserInteractions.GoTo(
					$"{TestSiteConfigurationSetup.URL}wfm/#/resourceplanner/planninggroup/{AppConfigs.PlanningGroupId}/detail/{AppConfigs.PlanningPeriodId}");
				using (new TimeoutScope(browserActivator, TimeSpan.FromMinutes(30)))
				{
					browserInteractions.Click(".schedule-button:enabled");
				}

				using (new TimeoutScope(browserActivator, TimeSpan.FromDays(1)))
				{
					browserInteractions.AssertExistsUsingJQuery(
						".heatmap-div:visible, .test-errorMessage, .server-busy, #Login-container, .test-alert, .notice-warning");
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
				browserInteractions.AssertExistsUsingJQuery(".heatmap-div:visible");
				
				TestLog.Static.Debug(
					$"Agents with schedules/total number of agents in planning group: {browserInteractions.GetText(".scheduled-agents")}");
			}
		}
	}
}