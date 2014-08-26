using System.IO;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestDataSetup
	{
		private static DatabaseHelper.Backup _ccc7DataBackup;
		private static IDataSource datasource;
		private static IPerson personThatCreatesTestData;

		public static void CreateDataSource()
		{
			datasource = DataSourceHelper.CreateDataSource(new[] { new EventsMessageSender(new SyncEventsPublisher(new EventPublisher(new HardCodedResolver(), new EventContextPopulator(new CurrentIdentity(), new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make()))))) }, "TestData");
		}

		public static void SetupFakeState()
		{
			personThatCreatesTestData = PersonFactory.CreatePersonWithBasicPermissionInfo("UserThatCreatesTestData", DefaultPassword.ThePassword);
			DefaultBusinessUnit.BusinessUnitFromFakeState = new BusinessUnit("BusinessUnit");

			StateHolderProxyHelper.SetupFakeState(datasource, personThatCreatesTestData, DefaultBusinessUnit.BusinessUnitFromFakeState, new ThreadPrincipalContext(new TeleoptiPrincipalFactory()));

			GlobalPrincipalState.Principal = Thread.CurrentPrincipal as TeleoptiPrincipal;
			GlobalUnitOfWorkState.CurrentUnitOfWorkFactory = UnitOfWorkFactory.CurrentUnitOfWorkFactory();
		}

		public static void CreateMinimumTestData()
		{
			GlobalUnitOfWorkState.UnitOfWorkAction(createPersonThatCreatesTestData);
			GlobalUnitOfWorkState.UnitOfWorkAction(createLicense);
		}

		public static void ClearAnalyticsData()
		{
			DataSourceHelper.ClearAnalyticsData();
		}

		public static void BackupCcc7Data()
		{
			_ccc7DataBackup = DataSourceHelper.BackupCcc7DataByFileCopy("Teleopti.Ccc.WebBehaviorTest");
		}

		public static void RestoreCcc7Data()
		{
			Navigation.GoToWaitForUrlAssert("Test/ClearConnections", new ApplicationStartupTimeout());
			DataSourceHelper.RestoreCcc7DataByFileCopy(_ccc7DataBackup);
		}

		private static void createLicense(IUnitOfWork uow)
		{
			var license = new License {XmlString = File.ReadAllText("License.xml")};
			var licenseRepository = new LicenseRepository(uow);
			licenseRepository.Add(license);
		}

		private static void createPersonThatCreatesTestData(IUnitOfWork uow)
		{
			var personRepository = new PersonRepository(uow);
			personRepository.Add(personThatCreatesTestData);
		}
	}
}