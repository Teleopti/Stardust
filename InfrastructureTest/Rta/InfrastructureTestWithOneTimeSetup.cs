using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public class InfrastructureTestWithOneTimeSetup
	{
		[OneTimeSetUp]
		public void OneTimeSetupWithContainer()
		{
			var service = new IoCTestService(new[] { this }, null);

			var config = service.Config();
			config.FakeConnectionString("MessageBroker", InfraTestConfigReader.AnalyticsConnectionString);
			config.FakeConnectionString("Tenancy", InfraTestConfigReader.ConnectionString);
			config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);

			var toggles = service.Toggles();

			var builder = new ContainerBuilder();
			var system = new SystemImpl(builder, new TestDoubles());

			var args = new IocArgs(config);
			var configuration = new IocConfiguration(args, toggles);

			IoCTestAttribute.SetupSystem(system, configuration, config, toggles);
			InfrastructureTestAttribute.SetupSystem(system, configuration);
			(this as ISetup)?.Setup(system, configuration);

			var container = builder.Build();

			container.Resolve<HangfireClientStarter>().Start();

			service.InjectFrom(container);

			OneTimeSetUp();

			container.Dispose();
		}

		public virtual void OneTimeSetUp()
		{
		}

		[OneTimeTearDown]
		public void RestoreDatabases()
		{
			SetupFixtureForAssembly.RestoreCcc7Database();
			SetupFixtureForAssembly.RestoreAnalyticsDatabase();
		}
	}
}