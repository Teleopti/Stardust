﻿using System;
using System.IO;
using System.Linq;
using Autofac;
using log4net.Config;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Rta.PerformanceTest.Code;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[SetUpFixture]
	public class NUnitSetup
	{
		[OneTimeSetUp]
		public void Setup()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			XmlConfigurator.Configure();

			DataSourceHelper.CreateDatabases();

			TestSiteConfigurationSetup.Setup();
			
			IntegrationIoCTest.Setup(builder =>
			{
				builder.RegisterType<TestConfiguration>().SingleInstance();
				builder.RegisterType<DataCreator>().SingleInstance().ApplyAspects();
				builder.RegisterType<StatesSender>().SingleInstance().ApplyAspects();
				builder.RegisterType<ScheduleInvalidator>().SingleInstance().ApplyAspects();
				builder.RegisterType<ConfigurableSyncEventPublisher>().SingleInstance();
				builder.RegisterType<NoMessageSender>().As<IMessageSender>().SingleInstance();
			}, this);
			
		}
		
		[OneTimeTearDown]
		public void Teardown()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}