using System;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
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
			var userName = "demo";
			var password = "demo";
			var businessUnitName = "Teleopti WFM Demo";

			using (var browserActivator = new CoypuChromeActivator())
			{
				browserActivator.Start(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(5));
				var browserInteractions = browserActivator.GetInteractions();

				//Logon
				browserInteractions.GoTo(string.Concat(
					TestSiteConfigurationSetup.URL, 
					"Test/Logon", 
					string.Format("?businessUnitName={0}&userName={1}&password={2}", businessUnitName, userName, password)));
			}
		}

		[TearDown]
		public void CleanUp()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}