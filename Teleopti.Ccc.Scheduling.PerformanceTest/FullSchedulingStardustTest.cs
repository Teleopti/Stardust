using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver.CoypuImpl;

namespace Teleopti.Ccc.Scheduling.PerformanceTest
{
	[PerformanceTest]
	public class FullSchedulingStardustTest
	{
		public HangfireUtilities Hangfire;
		public TestLog TestLog;

		[Test]
		[Category("ScheduleOptimizationStardust")]
		public void MeasurePerformanceOnStardust()
		{
			Hangfire.CleanQueue();
			TestLog.Debug($"Number of succeeded jobs before scheduling {Hangfire.SucceededFromStatistics()}");
			var hangfireQueueLogCancellationToken = new CancellationTokenSource();
			Task.Run(() =>
			{
				HangfireLogger.LogHangfireQueues(TestLog, Hangfire);
			}, hangfireQueueLogCancellationToken.Token);

			using (var browserActivator = new CoypuChromeActivator())
			{
				//long timeout for now due to slow loading of planning period view on large dbs. Could be lowered when fixed
				browserActivator.Start(TimeSpan.FromSeconds(90), TimeSpan.FromMilliseconds(500));
				var browserInteractions = browserActivator.GetInteractions();

				WebAction.Logon(browserInteractions, AppConfigs.BusinessUnitName, AppConfigs.UserName, AppConfigs.Password);

				browserInteractions.GoTo($"{TestSiteConfigurationSetup.URL}wfm/#/resourceplanner/planninggroup/{AppConfigs.PlanningGroupId}/detail/{AppConfigs.PlanningPeriodId}");
				using (new TimeoutScope(browserActivator, TimeSpan.FromMinutes(30)))
				{
					browserInteractions.Click(".schedule-button:enabled");
				}

				browserInteractions.AssertExists(".test-schedule-is-running");

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

			TestLog.Debug($"Number of succeeded jobs before Hangfire.WaitForQueue {Hangfire.SucceededFromStatistics()}");
			Hangfire.WaitForQueue();
			hangfireQueueLogCancellationToken.Cancel();
			TestLog.Debug($"Number of succeeded jobs after Hangfire.WaitForQueue {Hangfire.SucceededFromStatistics()}");
		}
	}
}