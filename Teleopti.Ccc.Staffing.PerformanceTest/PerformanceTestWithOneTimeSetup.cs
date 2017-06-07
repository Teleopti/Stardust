using System;
using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.Staffing.PerformanceTest
{
	public class PerformanceTestWithOneTimeSetup 
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

			SetupSystem(system, configuration, config, toggles);

			var container = builder.Build();
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
		}

		//Register what's needed for OneTimeSetup here
		public static void SetupSystem(ISystem system, IocConfiguration configuration, FakeConfigReader config, FakeToggleManager toggles)
		{
			var intervalFetcher = new FakeIntervalLengthFetcher();
			intervalFetcher.Has(15);  //because we don't restore Analytics
			system.AddModule(new CommonModule(configuration));
			system.UseTestDouble(new MutableNow(new DateTime(2017, 02, 13, 8, 0, 0))).For<INow>();
			system.UseTestDouble<FakeTime>().For<ITime>();
			system.UseTestDouble(intervalFetcher).For<IIntervalLengthFetcher>();
			system.UseTestDouble(config).For<IConfigReader>();
			system.UseTestDouble(toggles).For<IToggleManager>();
			system.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
			system.UseTestDouble<NoMessageSender>().For<IMessageSender>();
			system.AddService<Database>();
			system.AddModule(new TenantServerModule(configuration));
		}
	}
}