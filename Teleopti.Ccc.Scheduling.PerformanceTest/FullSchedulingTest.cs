using NUnit.Framework;
using Teleopti.Ccc.TestCommon.Web.StartWeb;

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
			
		}

		[TearDown]
		public void CleanUp()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}