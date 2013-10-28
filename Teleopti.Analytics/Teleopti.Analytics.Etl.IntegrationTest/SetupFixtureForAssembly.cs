using System;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[SetUpFixture]
	public class SetupFixtureForAssembly
	{
		[SetUp]
		public void Setup()
		{
			var dataSource = DataSourceHelper.CreateDataSource(new IMessageSender[] { }, "TestData");

			var personThatCreatesTestData = PersonFactory.CreatePersonWithBasicPermissionInfo("UserThatCreatesTestData", "password");

			TestState.BusinessUnit = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
			TestState.BusinessUnit.Name = "BusinessUnit";

			StateHolderProxyHelper.SetupFakeState(dataSource, personThatCreatesTestData, TestState.BusinessUnit, new ThreadPrincipalContext(new TeleoptiPrincipalFactory()));

			using (var uow = UnitOfWorkFactory.CurrentUnitOfWorkFactory().LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var testDataFactory = new TestDataFactory(action =>
					{
						action.Invoke(uow);
						uow.PersistAll();
					});
				testDataFactory.Apply(new PersonThatCreatesTestData(personThatCreatesTestData));
				testDataFactory.Apply(new LicenseFromFile());
				testDataFactory.Apply(new BusinessUnitFromFakeState(TestState.BusinessUnit));
			}

			TestState.Ccc7DataBackup = DataSourceHelper.BackupCcc7DataByFileCopy("Teleopti.Analytics.Etl.IntegrationTest");
		}

		private static void DisposeUnitOfWork()
		{
			TestState.UnitOfWork.Dispose();
			TestState.UnitOfWork = null;
		}

		private static void OpenUnitOfWork()
		{
			TestState.UnitOfWork = UnitOfWorkFactory.CurrentUnitOfWorkFactory().LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork();
		}

		private static void UnitOfWorkAction(Action<IUnitOfWork> action)
		{
			action(TestState.UnitOfWork);
			TestState.UnitOfWork.PersistAll();
		}

		public static void BeginTest()
		{
			TestState.TestDataFactory = new TestDataFactory(UnitOfWorkAction);
			DataSourceHelper.RestoreCcc7DataByFileCopy(TestState.Ccc7DataBackup);
            DataSourceHelper.ClearAnalyticsData();
			OpenUnitOfWork();
		}

		public static void EndTest()
		{
			DisposeUnitOfWork();
		}
	}
}
