using System.IO;
using Autofac;
using log4net.Config;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Scheduling.PerformanceTest.Domain
{
	[SetUpFixture]
	public class NUnitSetup
	{
		[OneTimeSetUp]
		public void Setup()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			XmlConfigurator.Configure();
			TestSiteConfigurationSetup.Setup(true);
			
			IntegrationIoCTest.Setup(builder =>
			{
				var config = new FakeConfigReader();
				config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);
				builder.Register(c => config).As<IConfigReader>().SingleInstance();
			});
		}

		[OneTimeTearDown]
		public void CleanUp()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}