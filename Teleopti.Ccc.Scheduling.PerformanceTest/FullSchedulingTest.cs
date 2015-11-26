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
			using (var browserActivator = new CoypuChromeActivator())
			{
				browserActivator.Start(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(5));
				var browserInteractions = browserActivator.GetInteractions();
				browserInteractions.GoTo(new Uri(TestSiteConfigurationSetup.URL, "Authentication").ToString());
			}
		}

		[TearDown]
		public void CleanUp()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}