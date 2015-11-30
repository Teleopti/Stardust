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
		public void MeasurePerformance()
		{
			using (var browserActivator = new CoypuChromeActivator())
			{
				browserActivator.Start(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(100));
				var browserInteractions = browserActivator.GetInteractions();

				logon(browserInteractions, AppConfigs.BusinessUnitName, AppConfigs.UserName, AppConfigs.Password);

				scheduleAndOptimize(browserInteractions, AppConfigs.PlanningPeriodId);

				using (new TimeoutScope(browserActivator, TimeSpan.FromHours(10)))
				{
					browserInteractions.AssertExists(".scheduling-result, .errorMessage, .alert-warning");
				}
				browserInteractions.AssertNotExists(".scheduling-result", ".errorMessage");
				browserInteractions.AssertNotExists(".scheduling-result", ".alert-warning");
			}
		}

		private static void scheduleAndOptimize(IBrowserInteractions browserInteractions, string planningPeriodId)
		{
			browserInteractions.GoTo(string.Concat(TestSiteConfigurationSetup.URL, "wfm/#/resourceplanner/planningperiod/", planningPeriodId));
			browserInteractions.Click(".schedule-button");
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