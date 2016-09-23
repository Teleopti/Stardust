using System.Collections.Generic;
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
			Toggles.RTA_ScheduleProjectionReadOnlyHangfire_35703,

			// Analytics stuff based on events
			Toggles.ETL_SpeedUpIntradayBusinessUnit_38932,
			Toggles.ETL_SpeedUpScenario_38300,
			Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439,
			Toggles.ETL_EventbasedDate_39562,
			Toggles.ETL_SpeedUpIntradayPreference_37124,
			Toggles.ETL_SpeedUpIntradaySkill_37543,
			Toggles.SettingsForPersonPeriodChanged_ToHangfire_38207,
			Toggles.ETL_SpeedUpIntradayDayOff_38213,
			Toggles.ETL_SpeedUpIntradayActivity_38303,
			Toggles.ETL_SpeedUpIntradayOvertime_38304,
			Toggles.ETL_SpeedUpIntradayAbsence_38301,
			Toggles.ETL_SpeedUpIntradayShiftCategory_38718,
			Toggles.ETL_SpeedUpIntradayRequest_38914,
			Toggles.ETL_SpeedUpIntradayScorecard_38933,
			Toggles.ETL_SpeedUpIntradayAvailability_38926,
			Toggles.ETL_SpeedUpIntradayWorkload_38928,
			Toggles.ETL_SpeedUpIntradayDate_38934,
			Toggles.ETL_SpeedUpIntradayForecastWorkload_38929,
		};
	}
}