﻿using System.IO;
using Autofac;
using log4net;
using log4net.Config;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Rta.PerformanceTest.Code;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[SetUpFixture]
	public class NUnitSetup
	{
		private ICurrentTransactionHooks transactionHooks;
		private DefaultDataCreator defaultDataCreator;
		private DataCreator dataCreator;

		[SetUp]
		public void Setup()
		{
			XmlConfigurator.Configure();

			TestSiteConfigurationSetup.Setup();

			var builder = new ContainerBuilder();
			var args = new IocArgs(new ConfigReader())
			{
				AllEventPublishingsAsSync = true,
			};
			builder.RegisterModule(new CommonModule(new IocConfiguration(args, new FakeToggleManager())));
			builder.RegisterType<MutableNow>().AsSelf().As<INow>().SingleInstance();
			builder.RegisterType<DefaultDataCreator>().SingleInstance();
			builder.RegisterType<TestConfiguration>().SingleInstance();
			builder.RegisterType<Http>().SingleInstance();
			builder.RegisterType<DataCreator>().SingleInstance().ApplyAspects();
			builder.RegisterType<NoMessageSender>().As<IMessageSender>().SingleInstance();
			var container = builder.Build();

			transactionHooks = container.Resolve<ICurrentTransactionHooks>();
			defaultDataCreator = container.Resolve<DefaultDataCreator>();
			dataCreator = container.Resolve<DataCreator>();

			LogManager.GetLogger(typeof(NUnitSetup)).Debug("defaultDataCreator.HashValue " + defaultDataCreator.HashValue);
			LogManager.GetLogger(typeof(NUnitSetup)).Debug("TestConfiguration.HashValue " + TestConfiguration.HashValue);
			var dataHash = defaultDataCreator.HashValue ^ TestConfiguration.HashValue;
			var path = Path.Combine(InfraTestConfigReader.DatabaseBackupLocation, "Rta");

			var haveDatabases =
				DataSourceHelper.TryRestoreApplicationDatabaseBySql(path, dataHash) &&
				DataSourceHelper.TryRestoreAnalyticsDatabaseBySql(path, dataHash);
			if (!haveDatabases)
				createDatabase(path, dataHash);

			TestSiteConfigurationSetup.StartApplicationSync();
		}

		private void createDatabase(string path, int dataHash)
		{
			DataSourceHelper.CreateDatabases();

			StateHolderProxyHelper.SetupFakeState(
				DataSourceHelper.CreateDataSource(transactionHooks),
				DefaultPersonThatCreatesData.PersonThatCreatesDbData,
				DefaultBusinessUnit.BusinessUnit,
				new ThreadPrincipalContext()
				);

			defaultDataCreator.Create();
			dataCreator.Create();

			DataSourceHelper.BackupApplicationDatabaseBySql(path, dataHash);
			DataSourceHelper.BackupAnalyticsDatabaseBySql(path, dataHash);

			StateHolderProxyHelper.Logout(new ThreadPrincipalContext());
		}

		[TearDown]
		public void Teardown()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}