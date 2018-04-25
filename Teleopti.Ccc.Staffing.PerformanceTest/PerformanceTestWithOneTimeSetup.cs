﻿using System;
using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
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

			setupSystem(system, system, configuration, config, toggles);

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
		private static void setupSystem(IIsolate isolate, IExtend extend, IocConfiguration configuration, FakeConfigReader config, FakeToggleManager toggles)
		{
			var intervalFetcher = new FakeIntervalLengthFetcher();
			intervalFetcher.Has(15);  //because we don't restore Analytics
			extend.AddModule(new CommonModule(configuration));
			isolate.UseTestDouble(new MutableNow(new DateTime(2017, 02, 13, 8, 0, 0))).For<INow>();
			isolate.UseTestDouble<FakeTime>().For<ITime>();
			isolate.UseTestDouble(intervalFetcher).For<IIntervalLengthFetcher>();
			isolate.UseTestDouble(config).For<IConfigReader>();
			isolate.UseTestDouble(toggles).For<IToggleManager>();
			isolate.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
			isolate.UseTestDouble<NoMessageSender>().For<IMessageSender>();
			isolate.UseTestDouble<StaffingViewModelCreator>().For<StaffingViewModelCreator>();
			extend.AddService<Database>();
			extend.AddService<MultiplicatorDefinitionSetRepository>();
		}
	}
}