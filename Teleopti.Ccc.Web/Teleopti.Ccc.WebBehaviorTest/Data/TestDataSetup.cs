using System.IO;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestDataSetup
	{
		private static DatabaseHelper.Backup _Ccc7DataBackup;
		private static IDataSource datasource;
		private static IPerson personThatCreatesTestData;

		public static void CreateDataSource()
		{
			datasource = DataSourceHelper.CreateDataSource(new[] { new EventsMessageSender(new SyncEventsPublisher(new EventPublisher(new HardCodedResolver(), new EventContextPopulator(new CurrentIdentity(), new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make()))))) }, "TestData");
		}

		public static void SetupFakeState()
		{
			personThatCreatesTestData = PersonFactory.CreatePersonWithBasicPermissionInfo("UserThatCreatesTestData", TestData.CommonPassword);
			CommonBusinessUnit.BusinessUnitFromFakeState = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
			CommonBusinessUnit.BusinessUnitFromFakeState.Name = "BusinessUnit";

			StateHolderProxyHelper.SetupFakeState(datasource, personThatCreatesTestData, CommonBusinessUnit.BusinessUnitFromFakeState, new ThreadPrincipalContext(new TeleoptiPrincipalFactory()));

			GlobalPrincipalState.Principal = Thread.CurrentPrincipal as TeleoptiPrincipal;
			GlobalUnitOfWorkState.CurrentUnitOfWorkFactory = UnitOfWorkFactory.CurrentUnitOfWorkFactory();
		}

		public static void CreateMinimumTestData()
		{
			GlobalUnitOfWorkState.UnitOfWorkAction(CreatePersonThatCreatesTestData);
			GlobalUnitOfWorkState.UnitOfWorkAction(CreateLicense);
		}

		public static void ClearAnalyticsData()
		{
			DataSourceHelper.ClearAnalyticsData();
		}

		public static void BackupCcc7Data()
		{
			_Ccc7DataBackup = DataSourceHelper.BackupCcc7DataByFileCopy("Teleopti.Ccc.WebBehaviorTest");
		}

		public static void RestoreCcc7Data()
		{
			Navigation.GoToWaitForUrlAssert("Test/ClearConnections", "Test/ClearConnections", new ApplicationStartupTimeout());
			DataSourceHelper.RestoreCcc7DataByFileCopy(_Ccc7DataBackup);
		}

	    private static void CreateLicense(IUnitOfWork uow)
		{
			var license = new License {XmlString = File.ReadAllText("License.xml")};
			var licenseRepository = new LicenseRepository(uow);
			licenseRepository.Add(license);
		}

		private static void CreatePersonThatCreatesTestData(IUnitOfWork uow)
		{
			var personRepository = new PersonRepository(uow);
			personRepository.Add(personThatCreatesTestData);
		}

	}
}