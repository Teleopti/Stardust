using System;
using System.IO;
using System.Linq;
using Autofac;
using NUnit.Framework.Interfaces;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Rta.PerformanceTest.Code
{
	public class RtaPerformanceTestAttribute : IntegrationTestAttribute
	{
		public IComponentContext Container;
		public DefaultDataCreator DefaultDataCreator;
		public DataCreator DataCreator;
		public DefaultAnalyticsDataCreator DefaultAnalyticsDataCreator;
		public ImpersonateSystem Impersonate;
		public IDataSourceScope DataSource;
		public WithUnitOfWork WithUnitOfWork;
		public IBusinessUnitRepository BusinessUnits;
		public IHangfireClientStarter HangfireClientStarter;

		public override void BeforeTest(ITest testDetails)
		{
			base.BeforeTest(testDetails);

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
					DataSourceHelper.CreateDataSource(Container),
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
			using (DataSource.OnThisThreadUse(DataSourceHelper.TenantName))
				businessUnitId = WithUnitOfWork.Get(() => BusinessUnits.LoadAll().First()).Id.Value;
			Impersonate.Impersonate(DataSourceHelper.TenantName, businessUnitId);
			TestSiteConfigurationSetup.TearDown();

			TestSiteConfigurationSetup.Setup();
		}

		public override void AfterTest(ITest testDetails)
		{
			base.AfterTest(testDetails);

			TestSiteConfigurationSetup.TearDown();

			Impersonate?.EndImpersonation();

		}
	}
}