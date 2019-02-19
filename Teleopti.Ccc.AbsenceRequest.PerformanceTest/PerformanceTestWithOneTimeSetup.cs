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
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Reports.IoC;
using Teleopti.Ccc.Web.Areas.Requests.Core.IOC;
using Teleopti.Ccc.Web.Areas.TeamSchedule.IoC;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.AbsenceRequest.PerformanceTest
{	
	public class PerformanceTestWithOneTimeSetup 
	{
		[OneTimeSetUp]
		public void OneTimeSetupWithContainer()
		{
			var service = new IoCTestService(new[] { this }, null);

			var config = service.Config();
			config.FakeConnectionString("MessageBroker", InfraTestConfigReader.AnalyticsConnectionString());
			config.FakeConnectionString("Tenancy", InfraTestConfigReader.ApplicationConnectionString());
			config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString());

			var toggles = service.Toggles();

			var builder = new ContainerBuilder();
			var system = new SystemImpl(builder, new TestDoubles());

			var args = new IocArgs(config);
			var configuration = new IocConfiguration(args, toggles);

			setupSystem(system, system, configuration, config, toggles);

			var container = builder.Build();
			service.InjectFrom(container);

			OneTimeSetUp();

			container.Dispose();
		}

		public virtual void OneTimeSetUp()
		{
		}

		public virtual void OneTimeTearDown()
		{
		}

		[OneTimeTearDown]
		public void RestoreDatabases()
		{
			OneTimeTearDown();
		}

		//Register what's needed for OneTimeSetup here
		private static void setupSystem(IIsolate isolate, IExtend extend, IocConfiguration configuration, FakeConfigReader config, FakeToggleManager toggles)
		{
			extend.AddModule(new CommonModule(configuration));
			isolate.UseTestDouble(new MutableNow(new DateTime(2017, 02, 13, 8, 0, 0))).For<INow>();
			isolate.UseTestDouble<FakeTime>().For<ITime>();
			isolate.UseTestDouble(config).For<IConfigReader>();
			isolate.UseTestDouble(toggles).For<IToggleManager>();
			isolate.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
			isolate.UseTestDouble<NoMessageSender>().For<IMessageSender>();
			extend.AddService<Database>();
			extend.AddModule(new LegacyRegistrationsFromAnywhere(configuration));
			extend.AddModule(new RequestsAreaModule());
			extend.AddModule(new ReportsAreaModule());
			extend.AddModule(new TeamScheduleAreaModule());
		}
	}
}