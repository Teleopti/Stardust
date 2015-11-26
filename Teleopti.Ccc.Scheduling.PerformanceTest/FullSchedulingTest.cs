using System;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver.CoypuImpl;

namespace Teleopti.Ccc.Scheduling.PerformanceTest
{
	public class FullSchedulingTest
	{
		[SetUp]
		public void Setup()
		{
			TestSiteConfigurationSetup.Setup();
		}

		[Test, Explicit]
		public void MeasurePerformance()
		{
			// Move this to app.config!
			var userName = "demo";
			var password = "demo";
			var businessUnitName = "Teleopti WFM Demo";
			var planningPeriodId = "32f17285-f7ad-4c76-a050-a5590100ca00";
			///////////////////////////

			using (var browserActivator = new CoypuChromeActivator())
			{
				browserActivator.Start(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(100));
				var browserInteractions = browserActivator.GetInteractions();

				//Logon
				browserInteractions.GoTo(string.Concat(
					TestSiteConfigurationSetup.URL, 
					"Test/Logon", 
					string.Format("?businessUnitName={0}&userName={1}&password={2}", businessUnitName, userName, password)));

				//Goto scheduling page
				browserInteractions.GoTo(string.Concat(TestSiteConfigurationSetup.URL, "wfm/#/resourceplanner/planningperiod/", planningPeriodId));

				//click schedule button
				browserInteractions.Click(".schedule-button");

				//wrong assert here for now - should check
				//1. Show report page -> ok
				//2. If "server busy -> fail
				//3. If error is written -> fail
				browserInteractions.AssertExists(".schedule-report");
			}
		}

		[TearDown]
		public void CleanUp()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}