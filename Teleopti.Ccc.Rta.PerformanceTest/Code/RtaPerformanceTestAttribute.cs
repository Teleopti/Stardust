using System;
using System.IO;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Rta.PerformanceTest.Code
{
	public class RtaPerformanceTestAttribute : IntegrationIoCTestAttribute
	{
		public ICurrentTransactionHooks TransactionHooks;
		public DefaultDataCreator DefaultDataCreator;
		public DataCreator DataCreator;
		public DefaultAnalyticsDataCreator DefaultAnalyticsDataCreator;
		public ImpersonateSystem Impersonate;
		public IDataSourceScope DataSource;
		public WithUnitOfWork WithUnitOfWork;
		public IBusinessUnitRepository BusinessUnits;
		public HangfireClientStarter HangfireClientStarter;
		public HangfireUtilities Hangfire;

		protected override void BeforeTest()
		{
			base.BeforeTest();

			var dataHash = DefaultDataCreator.HashValue ^ TestConfiguration.HashValue;
			var path = Path.Combine(InfraTestConfigReader.DatabaseBackupLocation, "Rta");

			var haveDatabases =
				DataSourceHelper.TryRestoreApplicationDatabaseBySql(path, dataHash) &&
				DataSourceHelper.TryRestoreAnalyticsDatabaseBySql(path, dataHash);
			if (!haveDatabases)
				DataSourceHelper.CreateDatabases();

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

			HangfireClientStarter.Start();

			Guid businessUnitId;
			using (DataSource.OnThisThreadUse(DataSourceHelper.TestTenantName))
				businessUnitId = WithUnitOfWork.Get(() => BusinessUnits.LoadAll().First()).Id.Value;
			Impersonate.Impersonate(DataSourceHelper.TestTenantName, businessUnitId);

			TestSiteConfigurationSetup.Setup();
		}

		protected override void AfterTest()
		{
			base.AfterTest();

			TestSiteConfigurationSetup.TearDown();

			Impersonate?.EndImpersonation();

			Hangfire.WaitForQueue();
		}
	}
}