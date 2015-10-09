using System;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
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
			DataSource = DataSourceHelper.CreateDataSource(new NoMessageSenders(), UserConfigurable.DefaultTenantName);

			var personThatCreatesTestData = PersonFactory.CreatePerson("UserThatCreatesTestData", "password");

			TestState.BusinessUnit = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
			TestState.BusinessUnit.Name = "BusinessUnit";

			StateHolderProxyHelper.SetupFakeState(DataSource, personThatCreatesTestData, TestState.BusinessUnit, new ThreadPrincipalContext(new TeleoptiPrincipalFactory()));
			var tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(UnitOfWorkFactory.Current.ConnectionString);

			using (var uow = UnitOfWorkFactory.CurrentUnitOfWorkFactory().Current().CreateAndOpenUnitOfWork())
			{
				var testDataFactory = new TestDataFactory(action =>
					{
						action.Invoke(new ThisUnitOfWork(uow));
						uow.PersistAll();
					},
					tenantAction =>
					{
						using (tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted())
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

		private static void DisposeUnitOfWork()
		{
			TestState.UnitOfWork.Dispose();
			TestState.UnitOfWork = null;
		}

		private static void OpenUnitOfWork()
		{
			TestState.UnitOfWork = UnitOfWorkFactory.CurrentUnitOfWorkFactory().Current().CreateAndOpenUnitOfWork();
		}

		private static void UnitOfWorkAction(Action<ICurrentUnitOfWork> action)
		{
			action(new ThisUnitOfWork(TestState.UnitOfWork));
			TestState.UnitOfWork.PersistAll();
		}
		
		public static void BeginTest()
		{
			var tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(UnitOfWorkFactory.Current.ConnectionString);

			TestState.TestDataFactory = new TestDataFactory(UnitOfWorkAction,
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
