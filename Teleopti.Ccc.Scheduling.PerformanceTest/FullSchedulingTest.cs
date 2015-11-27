using System;
using System.Configuration;
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
			var userName = ConfigurationManager.AppSettings["UserName"];
			var password = ConfigurationManager.AppSettings["Password"];
			var businessUnitName = ConfigurationManager.AppSettings["BusinessUnitName"];
			var planningPeriodId = ConfigurationManager.AppSettings["PlanningPeriodId"];

			using (var browserActivator = new CoypuChromeActivator())
			{
				browserActivator.Start(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(100));
				var browserInteractions = browserActivator.GetInteractions();

				//Logon
				browserInteractions.GoTo(string.Concat(
					TestSiteConfigurationSetup.URL, 
					"Test/Logon", 
					string.Format("?businessUnitName={0}&userName={1}&password={2}", businessUnitName, userName, password)));

				//Schedule a certain planning period
				browserInteractions.GoTo(string.Concat(TestSiteConfigurationSetup.URL, "wfm/#/resourceplanner/planningperiod/", planningPeriodId));
				browserInteractions.Click(".schedule-button");

				//Wait for scheduler to finish
				using (new TimeoutScope(browserActivator, TimeSpan.FromHours(10)))
				{
					//add checks for "server busy" or ".errorMessage" is visible during wait
					browserInteractions.AssertExists(".scheduling-result");
				}
			}
		}

		[TearDown]
		public void CleanUp()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}