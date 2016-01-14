using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
				browserActivator.Start(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(500));
				var browserInteractions = browserActivator.GetInteractions();

				logon(browserInteractions, AppConfigs.BusinessUnitName, AppConfigs.UserName, AppConfigs.Password);

				scheduleAndOptimize(browserInteractions, AppConfigs.PlanningPeriodId);

				using (new TimeoutScope(browserActivator, TimeSpan.FromDays(2)))
				{
					try
					{
						dumpInfo(browserInteractions, "Waiting.txt");
						browserInteractions.AssertExists(".scheduling-result, .test-errorMessage, .server-busy, #Login-container, .test-alert");
					}
					finally
					{
						dumpInfo(browserInteractions, "finally.txt");
					}
				}
				//no server error
				browserInteractions.AssertNotExists("body", ".test-errorMessage");
				//no failing request
				browserInteractions.AssertNotExists("body", ".test-alert");
				//not showing "server busy"
				browserInteractions.AssertNotExists("body", ".server-busy");
				//not redirected to logon page
				browserInteractions.AssertNotExists("body", "#Login-container");
				browserInteractions.AssertExists(".scheduling-result");
			}
		}

		private static void dumpInfo(IBrowserInteractions browserInteractions, string fileName)
		{
			var path = Path.Combine(Environment.CurrentDirectory, @"..\..\..\Logs\");
			var output = new List<string>();
			browserInteractions.DumpInfo(info =>
			{
				output.Add(info);
			});
			File.WriteAllLines(path + fileName, output);
		}


		private static void scheduleAndOptimize(IBrowserInteractions browserInteractions, string planningPeriodId)
		{
			browserInteractions.GoTo(string.Concat(TestSiteConfigurationSetup.URL, "wfm/#/resourceplanner/planningperiod/", planningPeriodId));
			//put here to dump better info
			//temp to see if this hack makes it more stable
			dumpInfo(browserInteractions, "JustAfterGoingToView.txt");
			Thread.Sleep(5000);
			browserInteractions.AssertExists(".schedule-button:enabled");
			dumpInfo(browserInteractions, "BeforeButtonClick.txt");
			browserInteractions.Click(".schedule-button:enabled");
			dumpInfo(browserInteractions, "AfterButtonClick.txt");
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