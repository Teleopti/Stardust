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
			XmlConfigurator.Configure();
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			TestSiteConfigurationSetup.Setup();
		}

		[OneTimeTearDown]
		public void CleanUp()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}