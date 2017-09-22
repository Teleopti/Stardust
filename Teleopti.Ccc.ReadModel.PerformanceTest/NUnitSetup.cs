using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Autofac;
using log4net.Config;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.ReadModel.PerformanceTest
{
	[SetUpFixture]
	public class NUnitSetup
	{
		private DefaultDataCreator defaultDataCreator;
		private DataCreator dataCreator;
		private AnalyticsDatabase analyticsDatabase;

		[OneTimeSetUp]
		public void Setup()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			XmlConfigurator.Configure();

			TestSiteConfigurationSetup.Setup();

			var builder = new ContainerBuilder();
			var args = new IocArgs(new ConfigReader())
			{
				AllEventPublishingsAsSync = true,
			};
			var fakeToggleManager = new FakeToggleManager();
			enabledTogglesOnStartup
				.ForEach(fakeToggleManager.Enable);
			var configuration = new IocConfiguration(args, fakeToggleManager);
			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterType<MutableNow>().AsSelf().As<INow>().SingleInstance();
			builder.RegisterType<DefaultDataCreator>().SingleInstance();
			builder.RegisterType<TestConfiguration>().SingleInstance();
			builder.RegisterType<Http>().SingleInstance();
			builder.RegisterType<AnalyticsDatabase>();
			builder.RegisterType<Database>().SingleInstance().ApplyAspects();
			builder.RegisterType<DataCreator>().SingleInstance().ApplyAspects();
			builder.RegisterModule(new TenantServerModule(configuration));
			builder.RegisterType<FakeMessageSender>().As<IMessageSender>();

			var container = builder.Build();
			
			defaultDataCreator = container.Resolve<DefaultDataCreator>();
			dataCreator = container.Resolve<DataCreator>();
			analyticsDatabase = container.Resolve<AnalyticsDatabase>();

			var dataHash = DefaultDataCreator.HashValue ^ TestConfiguration.HashValue;
			var path = Path.Combine(InfraTestConfigReader.DatabaseBackupLocation, "ReadModel");

			var haveDatabases =
				DataSourceHelper.TryRestoreApplicationDatabaseBySql(path, dataHash) &&
				DataSourceHelper.TryRestoreAnalyticsDatabaseBySql(path, dataHash);
			if (!haveDatabases)
				createDatabase(path, dataHash, container.Resolve<ICurrentTransactionHooks>());

			TestSiteConfigurationSetup.StartApplicationSync();
		}

		private void createDatabase(string path, int dataHash, ICurrentTransactionHooks currentTransactionHooks)
		{
			DataSourceHelper.CreateDatabases();

			StateHolderProxyHelper.SetupFakeState(
				DataSourceHelper.CreateDataSource(currentTransactionHooks),
				DefaultPersonThatCreatesData.PersonThatCreatesDbData,
				DefaultBusinessUnit.BusinessUnit
				);

			analyticsDatabase
				.WithQuarterOfAnHourInterval()
				.WithEternityAndNotDefinedDate()
				.WithDefaultSkillset()
				.WithDefaultAcdLogin();
			defaultDataCreator.Create();
			dataCreator.Create();

			DataSourceHelper.BackupApplicationDatabaseBySql(path, dataHash);
			DataSourceHelper.BackupAnalyticsDatabaseBySql(path, dataHash);

			StateHolderProxyHelper.Logout();
		}

		[OneTimeTearDown]
		public void Teardown()
		{
			TestSiteConfigurationSetup.TearDown();
		}

		private readonly List<Toggles> enabledTogglesOnStartup = new List<Toggles>
		{
			// Analytics stuff based on events
			Toggles.ETL_SpeedUpIntradayBusinessUnit_38932,
			Toggles.ETL_EventbasedDate_39562,
		};

		public static void LogHangfireQueues(TestLog testLog, HangfireUtilities hangfireUtilities)
		{
			while (true)
			{
				testLog.Debug($"Hangfire is processing {hangfireUtilities.NumberOfProcessingJobs()} jobs, {hangfireUtilities.NumberOfScheduledJobs()} are scheduled and {hangfireUtilities.NumberOfFailedJobs()} jobs has failed.");
				foreach (var queueName in Queues.OrderOfPriority())
				{
					testLog.Debug($"{hangfireUtilities.NumberOfJobsInQueue(queueName)} jobs in queue '{queueName}'");
				}
				Thread.Sleep(TimeSpan.FromSeconds(60));
			}
		}
	}
}