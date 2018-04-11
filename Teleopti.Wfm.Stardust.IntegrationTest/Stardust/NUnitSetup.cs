﻿using System.IO;
using Autofac;
using log4net.Config;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Messaging.Client;

namespace Teleopti.Wfm.Stardust.IntegrationTest.Stardust
{
	[SetUpFixture]
	public class NUnitSetup
	{
		[OneTimeSetUp]
		public void Setup()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			XmlConfigurator.Configure();

			//DataSourceHelper.CreateDatabases();

			TestSiteConfigurationSetup.Setup();

			IntegrationIoCTest.Setup(builder =>
			{
				builder.RegisterType<TestConfigReader>().As<IConfigReader>().SingleInstance();
				builder.RegisterType<DataCreator>().SingleInstance().ApplyAspects();
//				builder.RegisterType<StatesSender>().SingleInstance().ApplyAspects();
//				builder.RegisterType<ScheduleInvalidator>().SingleInstance().ApplyAspects();
				
				builder.RegisterType<FakeEventPublisher>().SingleInstance();
				builder.RegisterType<NoMessageSender>().As<IMessageSender>().SingleInstance();
			}, this);

			TestSiteConfigurationSetup.TearDown();
		}
		
	}
}