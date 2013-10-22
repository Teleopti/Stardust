using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[SetUpFixture]
	public class SetupFixtureForAssembly
	{
		private ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private static DatabaseHelper.Backup _Ccc7DataBackup;

		private void UnitOfWorkAction(Action<IUnitOfWork> action)
		{
			using (var uow = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				action(uow);
				uow.PersistAll();
			}
		}

		[SetUp]
		public void Setup()
		{
			var dataSource = DataSourceHelper.CreateDataSource(new IMessageSender[] { }, "TestData");

			var personThatCreatesTestData = PersonFactory.CreatePersonWithBasicPermissionInfo("UserThatCreatesTestData", "password");

			var businessUnitFromFakeState = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
			businessUnitFromFakeState.Name = "BusinessUnit";
            
			StateHolderProxyHelper.SetupFakeState(dataSource, personThatCreatesTestData, businessUnitFromFakeState, new ThreadPrincipalContext(new TeleoptiPrincipalFactory()));

			_unitOfWorkFactory = UnitOfWorkFactory.CurrentUnitOfWorkFactory();

			DataFactoryState.TestDataFactory = new TestDataFactory(UnitOfWorkAction);
		    
			Data.Apply(new PersonThatCreatesTestData(personThatCreatesTestData));
			Data.Apply(new LicenseFromFile());
            using (var uow = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
            {
                new BusinessUnitRepository(uow).Add(businessUnitFromFakeState);
                uow.PersistAll();
            }
			_Ccc7DataBackup = DataSourceHelper.BackupCcc7DataByFileCopy("Teleopti.Analytics.Etl.IntegrationTest");
		}

		public static void SetupDatabase()
		{
			DataSourceHelper.RestoreCcc7DataByFileCopy(_Ccc7DataBackup);
		}
	}
}
