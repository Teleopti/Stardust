using NUnit.Framework;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Scheduling.PerformanceTest
{
	[SetUpFixture]
	public class NUnitSetup
	{
		[OneTimeSetUp]
		public void Setup()
		{
			TestSiteConfigurationSetup.Setup();
		}

		[OneTimeTearDown]
		public void CleanUp()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}