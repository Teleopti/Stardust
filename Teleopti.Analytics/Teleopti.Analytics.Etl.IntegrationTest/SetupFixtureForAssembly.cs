using System;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[SetUpFixture]
	public class SetupFixtureForAssembly
	{
		public static IDataSource DataSource;

		[SetUp]
		public void Setup()
		{
			DataSource = DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");

			var personThatCreatesTestData = PersonFactory.CreatePerson("UserThatCreatesTestData", "password");

			TestState.BusinessUnit = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
			TestState.BusinessUnit.Name = "BusinessUnit";

			StateHolderProxyHelper.SetupFakeState(DataSource, personThatCreatesTestData, TestState.BusinessUnit, new ThreadPrincipalContext(new TeleoptiPrincipalFactory()));
			var tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(UnitOfWorkFactory.Current.ConnectionString);

			using (var uow = UnitOfWorkFactory.CurrentUnitOfWorkFactory().Current().CreateAndOpenUnitOfWork())
			{
				var testDataFactory = new TestDataFactory(defaultTenant(tenantUnitOfWorkManager), action =>
					{
						action.Invoke(uow);
						uow.PersistAll();
					},
					tenantAction =>
					{
						using (tenantUnitOfWorkManager.Start())
						{
							tenantAction(tenantUnitOfWorkManager);
						}
					});
				testDataFactory.Apply(new PersonThatCreatesTestData(personThatCreatesTestData));
				testDataFactory.Apply(new LicenseFromFile());
				testDataFactory.Apply(new BusinessUnitFromFakeState(TestState.BusinessUnit));
			}

			DataSourceHelper.BackupCcc7Database(123);
		}

		private static Tenant _defaultTenant;
		private static Tenant defaultTenant(TenantUnitOfWorkManager tenantUnitOfWorkManager)
		{
			using (tenantUnitOfWorkManager.Start())
			{
				return _defaultTenant ??
							 (_defaultTenant = tenantUnitOfWorkManager.CurrentSession().CreateQuery("select t from Tenant t where t.id=1").UniqueResult<Tenant>());
			}
		}

		private static void DisposeUnitOfWork()
		{
			TestState.UnitOfWork.Dispose();
			TestState.UnitOfWork = null;
		}

		private static void OpenUnitOfWork()
		{
			TestState.UnitOfWork = UnitOfWorkFactory.CurrentUnitOfWorkFactory().Current().CreateAndOpenUnitOfWork();
		}

		private static void UnitOfWorkAction(Action<IUnitOfWork> action)
		{
			action(TestState.UnitOfWork);
			TestState.UnitOfWork.PersistAll();
		}
		
		public static void BeginTest()
		{
			var tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(UnitOfWorkFactory.Current.ConnectionString);

			TestState.TestDataFactory = new TestDataFactory(defaultTenant(tenantUnitOfWorkManager), UnitOfWorkAction,
					tenantAction =>
					{
						tenantAction(tenantUnitOfWorkManager);
						tenantUnitOfWorkManager.CommitAndDisposeCurrent();
					});
			DataSourceHelper.RestoreCcc7Database(123);
      DataSourceHelper.ClearAnalyticsData();
			OpenUnitOfWork();
		}

		public static void EndTest()
		{
			DisposeUnitOfWork();
		}
	}
}
