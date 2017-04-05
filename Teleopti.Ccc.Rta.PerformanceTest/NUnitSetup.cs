using System;
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
		public ICurrentTransactionHooks TransactionHooks;
		public DefaultDataCreator DefaultDataCreator;
		public DataCreator DataCreator;
		public DefaultAnalyticsDataCreator DefaultAnalyticsDataCreator;
		public HangfireClientStarter HangfireClientStarter;
		public ImpersonateSystem Impersonate;
		public IDataSourceScope DataSource;
		public WithUnitOfWork WithUnitOfWork;
		public IBusinessUnitRepository BusinessUnits;

		[OneTimeSetUp]
		public void Setup()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			XmlConfigurator.Configure();

			TestSiteConfigurationSetup.Setup();

			var dataHash = DefaultDataCreator.HashValue ^ TestConfiguration.HashValue;
			var path = Path.Combine(InfraTestConfigReader.DatabaseBackupLocation, "Rta");

			var haveDatabases =
				DataSourceHelper.TryRestoreApplicationDatabaseBySql(path, dataHash) &&
				DataSourceHelper.TryRestoreAnalyticsDatabaseBySql(path, dataHash);
			if (!haveDatabases)
				DataSourceHelper.CreateDatabases();

			IntegrationIoCTest.Setup(builder =>
			{
				builder.RegisterType<TestConfiguration>().SingleInstance();
				builder.RegisterType<DataCreator>().SingleInstance().ApplyAspects();
				builder.RegisterType<StatesSender>().SingleInstance().ApplyAspects();
				builder.RegisterType<ScheduleInvalidator>().SingleInstance().ApplyAspects();
				builder.RegisterType<ConfigurableSyncEventPublisher>().SingleInstance();
				builder.RegisterType<NoMessageSender>().As<IMessageSender>().SingleInstance();
			}, this);

			HangfireClientStarter.Start();

			if (!haveDatabases)
			{
				StateHolderProxyHelper.SetupFakeState(
					DataSourceHelper.CreateDataSource(TransactionHooks),
					DefaultPersonThatCreatesData.PersonThatCreatesDbData,
					DefaultBusinessUnit.BusinessUnit
				);

				DefaultDataCreator.Create();
				DefaultAnalyticsDataCreator.OneTimeSetup();
				DataSourceHelper.ClearAnalyticsData();
				DefaultAnalyticsDataCreator.Create();
				DataCreator.Create();

				DataSourceHelper.BackupApplicationDatabaseBySql(path, dataHash);
				DataSourceHelper.BackupAnalyticsDatabaseBySql(path, dataHash);

				StateHolderProxyHelper.Logout();
			}

			Guid businessUnitId;
			using (DataSource.OnThisThreadUse(DataSourceHelper.TestTenantName))
				businessUnitId = WithUnitOfWork.Get(() => BusinessUnits.LoadAll().First()).Id.Value;
			Impersonate.Impersonate(DataSourceHelper.TestTenantName, businessUnitId);
		}
		
		[OneTimeTearDown]
		public void Teardown()
		{
			TestSiteConfigurationSetup.TearDown();
		}
	}
}