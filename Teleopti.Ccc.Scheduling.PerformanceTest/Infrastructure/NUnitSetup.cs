using System.IO;
using Autofac;
using log4net.Config;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Scheduling.PerformanceTest.Infrastructure
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

			IntegrationIoCTest.Setup(builder =>
			{
				var config = new FakeConfigReader();
				config.FakeConnectionString("Tenancy", InfraTestConfigReader.ConnectionString);
				config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);
				config.FakeSetting("OptimizeScheduleChangedEvents_DontUseFromWeb", "true");
				builder.Register(c => config).As<IConfigReader>().SingleInstance();

				//is this something we should fake?!
				builder.RegisterType<FakeMessageSender>().As<IMessageSender>().SingleInstance();
			});
		}

		[OneTimeTearDown]
		public void CleanUp()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}