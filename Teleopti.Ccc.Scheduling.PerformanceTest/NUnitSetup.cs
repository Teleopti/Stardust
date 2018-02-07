using System.IO;
using log4net.Config;
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
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			XmlConfigurator.Configure();
			TestSiteConfigurationSetup.Setup();
		}

		[OneTimeTearDown]
		public void CleanUp()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}