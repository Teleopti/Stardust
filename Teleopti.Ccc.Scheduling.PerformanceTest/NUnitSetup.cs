using NUnit.Framework;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Scheduling.PerformanceTest
{
	[SetUpFixture]
	public class NUnitSetup
	{
		[SetUp]
		public void Setup()
		{
			TestSiteConfigurationSetup.Setup();
		}

		[TearDown]
		public void CleanUp()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}