using NUnit.Framework;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[SetUpFixture]
	public class NUnitSetup
	{
		[SetUp]
		public void Setup()
		{
			TestSiteConfigurationSetup.Setup(TestSiteConfigurationSetup.PathToIISExpress64);

			TestDataSetup.Setup();
		}

		[TearDown]
		public void CleanUp()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}