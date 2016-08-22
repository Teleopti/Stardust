using System.IO;
using Autofac;
using log4net.Config;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ReadModel.PerformanceTest
{
	[SetUpFixture]
	public class NUnitSetup
	{
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
			var fakeToggleManager = new FakeToggleManager();
			fakeToggleManager.Enable(Toggles.RTA_ScheduleProjectionReadOnlyHangfire_35703);
			fakeToggleManager.Enable(Toggles.ETL_SpeedUpIntradayBusinessUnit_38932);
			fakeToggleManager.Enable(Toggles.ETL_SpeedUpScenario_38300);
			fakeToggleManager.Enable(Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439);
			fakeToggleManager.Enable(Toggles.PersonCollectionChanged_ToHangfire_38420);
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
			

			var dataHash = defaultDataCreator.HashValue ^ TestConfiguration.HashValue;
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

			defaultDataCreator.Create();
			dataCreator.Create();

			DataSourceHelper.BackupApplicationDatabaseBySql(path, dataHash);
			DataSourceHelper.BackupAnalyticsDatabaseBySql(path, dataHash);

			StateHolderProxyHelper.Logout();
		}

		[TearDown]
		public void Teardown()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}