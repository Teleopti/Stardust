using System;
using System.IO;
using System.Linq;
using Autofac;
using NUnit.Framework.Interfaces;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Wfm.Stardust.IntegrationTest.Stardust
{
	public class StardustTestAttribute : IntegrationIoCTestAttribute
	{
		public IComponentContext Container;
		public DefaultDataCreator DefaultDataCreator;
		public DataCreator DataCreator;
		public DefaultAnalyticsDataCreator DefaultAnalyticsDataCreator;
		public ImpersonateSystem Impersonate;
		public IDataSourceScope DataSource;
		public WithUnitOfWork WithUnitOfWork;
		public IBusinessUnitRepository BusinessUnits;
		public HangfireClientStarter HangfireClientStarter;
		public HangfireUtilities Hangfire;
		public StateQueueUtilities StateQueue;
		public IConfigReader ConfigReader;

		public override void BeforeTest(ITest testDetails)
		{
			base.BeforeTest(testDetails);

				var dataHash = DefaultDataCreator.HashValue;// ^ TestConfiguration.HashValue;
			var path = Path.Combine(InfraTestConfigReader.DatabaseBackupLocation, "Stardust");

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
				//DefaultAnalyticsDataCreator.OneTimeSetup();
				DataSourceHelper.ClearAnalyticsData();
				DefaultAnalyticsDataCreator.Create();
				DataCreator.Create();

				//DataSourceHelper.BackupApplicationDatabaseBySql(path, dataHash);
				//DataSourceHelper.BackupAnalyticsDatabaseBySql(path, dataHash);

				StateHolderProxyHelper.Logout();
			}

			HangfireClientStarter.Start();

			Guid businessUnitId;
			using (DataSource.OnThisThreadUse(DataSourceHelper.TestTenantName))
				businessUnitId = WithUnitOfWork.Get(() => BusinessUnits.LoadAll().First()).Id.Value;
			Impersonate.Impersonate(DataSourceHelper.TestTenantName, businessUnitId);

			TestSiteConfigurationSetup.Setup();
			((TestConfigReader)ConfigReader).ConfigValues.Add("ManagerLocation", TestSiteConfigurationSetup.URL.AbsoluteUri + @"StardustDashboard/");
			((TestConfigReader)ConfigReader).ConfigValues.Add("NumberOfNodes", "1");
			//ConfigReader.
			//"ManagerLocation", TestSiteConfigurationSetup.URL.AbsoluteUri + @"StardustDashboard/"
		}

		public override void AfterTest(ITest testDetails)
		{
			base.AfterTest(testDetails);

			Hangfire.WaitForQueue();
			//StateQueue.WaitForQueue();

			TestSiteConfigurationSetup.TearDown();

			Impersonate?.EndImpersonation();

		}
	}
}