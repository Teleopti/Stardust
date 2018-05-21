using System.IO;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;

namespace Teleopti.Wfm.Test
{
	[SetUpFixture]
	public class SetupFixtureForAssembly
	{
		public static IDataSource DataSource;

		[OneTimeSetUp]
		public void Setup()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			DataSource = DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());

			var personThatCreatesTestData = PersonFactory.CreatePerson("UserThatCreatesTestData", "password");

			TestState.BusinessUnit = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
			TestState.BusinessUnit.Name = "BusinessUnit";

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


		public static void BeginTest()
		{
			DataSourceHelper.RestoreApplicationDatabase(123);
			DataSourceHelper.ClearAnalyticsData();
		}

		public static void EndTest()
		{
			disposeUnitOfWork();
		}
	}
}