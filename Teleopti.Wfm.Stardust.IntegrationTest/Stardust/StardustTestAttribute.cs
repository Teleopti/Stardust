using System;
using System.IO;
using System.Linq;
using Autofac;
using NUnit.Framework.Interfaces;
using Teleopti.Ccc.Domain.Aop;
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
	public class StardustTestAttribute : IntegrationTestAttribute
	{
		public IComponentContext Container;
		public DefaultDataCreator DefaultDataCreator;
		public DataCreator DataCreator;
		public DefaultAnalyticsDataCreator DefaultAnalyticsDataCreator;
		public AsSystem AsSystem;
		public IDataSourceScope DataSource;
		public WithUnitOfWork WithUnitOfWork;
		public IBusinessUnitRepository BusinessUnits;
		public HangfireClientStarter HangfireClientStarter;
		public HangfireUtilities Hangfire;
		public StateQueueUtilities StateQueue;
		public IConfigReader ConfigReader;
		public TestLog TestLog;

		public override void BeforeTest(ITest testDetails)
		{
			base.BeforeTest(testDetails);

			var dataHash = DefaultDataCreator.HashValue;

			DataSourceHelper.CreateDatabases();
#if DEBUG
			var path = "./";
#else
			var path = Path.Combine(InfraTestConfigReader.DatabaseBackupLocation, "Stardust");
#endif

			var haveAnalyticsDatabase = DataSourceHelper.TryRestoreAnalyticsDatabaseBySql(path, dataHash);
			var haveAppDatabase = DataSourceHelper.TryRestoreApplicationDatabaseBySql(path, dataHash);

			//DO NOT remove this as you will get optimistic lock on two diff tests
			if (!haveAnalyticsDatabase)
			{
				try
				{
					TestLog.Debug("Setting up analytics data for the test");
					StateHolderProxyHelper.SetupFakeState(
						DataSourceHelper.CreateDataSource(Container),
						DefaultPersonThatCreatesData.PersonThatCreatesDbData,
						DefaultBusinessUnit.BusinessUnit
					);

					DataSourceHelper.ClearAnalyticsData();
					DefaultAnalyticsDataCreator.Create();
					
					DataSourceHelper.BackupAnalyticsDatabaseBySql(path, dataHash);

					StateHolderProxyHelper.Logout();
					StateHolderProxyHelper.ClearStateHolder();
				}
				catch (Exception ex)
				{
					TestLog.Debug(ex.InnerException.StackTrace);
					throw;
				}
			}
			if (!haveAppDatabase)
			{
				try
				{
					TestLog.Debug("Setting up data for the test");
					StateHolderProxyHelper.SetupFakeState(
						DataSourceHelper.CreateDataSource(Container),
						DefaultPersonThatCreatesData.PersonThatCreatesDbData,
						DefaultBusinessUnit.BusinessUnit
					);

					DefaultDataCreator.Create();
					DataCreator.Create();

					DataSourceHelper.BackupApplicationDatabaseBySql(path, dataHash);
					
					StateHolderProxyHelper.Logout();
					StateHolderProxyHelper.ClearStateHolder();
				}
				catch (Exception ex)
				{
					TestLog.Debug(ex.InnerException.StackTrace);
					throw;
				}
			}

			TestLog.Debug("Starting hangfire");
			HangfireClientStarter.Start();

			Guid businessUnitId;
			using (DataSource.OnThisThreadUse(DataSourceHelper.TestTenantName))
				businessUnitId = WithUnitOfWork.Get(() => BusinessUnits.LoadAll().First()).Id.Value;
			AsSystem.Logon(DataSourceHelper.TestTenantName, businessUnitId);

			TestSiteConfigurationSetup.Setup();
			var configReader = (TestConfigReader) ConfigReader;
			configReader.ConfigValues.Remove("ManagerLocation");
			configReader.ConfigValues.Remove("MessageBroker");
			configReader.ConfigValues.Remove("NumberOfNodes");

			configReader.ConfigValues.Add("ManagerLocation", TestSiteConfigurationSetup.URL.AbsoluteUri + @"StardustDashboard/");
			configReader.ConfigValues.Add("MessageBroker", TestSiteConfigurationSetup.URL.AbsoluteUri );
			configReader.ConfigValues.Add("NumberOfNodes", "1");
		}
		
		public override void AfterTest(ITest testDetails)
		{
			base.AfterTest(testDetails);
			TestLog.Debug("In teardown stopping all services");
			Hangfire.WaitForQueue();
			StateQueue.WaitForQueue();
			
			TestSiteConfigurationSetup.TearDown();
		}
	}
}