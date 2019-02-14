using System;
using System.IO;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[SetUpFixture]
	public class SetupFixtureForAssembly
	{
		public static IDataSource DataSource;
		private static IDisposable auth;

		[OneTimeSetUp]
		public void Setup()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			DataSource = DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());

			var personThatCreatesTestData = PersonFactory.CreatePerson("UserThatCreatesTestData", "password");

			TestState.BusinessUnit = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
			TestState.BusinessUnit.Name = "BusinessUnit";

			StateHolderProxyHelper.PrincipalFactory = new TeleoptiPrincipalForLegacyFactory();
			StateHolderProxyHelper.SetupFakeState(DataSource, personThatCreatesTestData, TestState.BusinessUnit);
			var tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(UnitOfWorkFactory.Current.ConnectionString);

			using (var uow = UnitOfWorkFactory.CurrentUnitOfWorkFactory().Current().CreateAndOpenUnitOfWork())
			{
				var testDataFactory = TestDataFactory.Make(uow, tenantUnitOfWorkManager);
				testDataFactory.Apply(new PersonThatCreatesTestData(personThatCreatesTestData));
				testDataFactory.Apply(new DefaultLicense());
				testDataFactory.Apply(new BusinessUnitFromFakeState(TestState.BusinessUnit));
			}

			DataSourceHelper.BackupApplicationDatabase(123);
		}

		private static void disposeUnitOfWork()
		{
			TestState.UnitOfWork.Dispose();
			TestState.UnitOfWork = null;
		}

		private static void openUnitOfWork()
		{
			TestState.UnitOfWork = UnitOfWorkFactory.CurrentUnitOfWorkFactory().Current().CreateAndOpenUnitOfWork();
		}

		public static void BeginTest()
		{
			DataSourceHelper.RestoreApplicationDatabase(123);
			DataSourceHelper.ClearAnalyticsData();

			openUnitOfWork();
			auth = CurrentAuthorization.ThreadlyUse(new FullPermission());
			var tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(UnitOfWorkFactory.Current.ConnectionString);
			TestState.TestDataFactory = TestDataFactory.Make(TestState.UnitOfWork, tenantUnitOfWorkManager);
		}

		public static void EndTest()
		{
			auth?.Dispose();
			disposeUnitOfWork();
		}
	}
}