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

		public static void CreateDataSource()
		{
			datasource = DataSourceHelper.CreateDataSource(new[] { new EventsMessageSender(new SyncEventsPublisher(new EventPublisher(new HardCodedResolver(), new EventContextPopulator(new CurrentIdentity(), new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make()))))) }, "TestData");

			setupFakeState();
			createGlobalDbData();

			backupCcc7Data();
		}

		private static void createGlobalDbData()
		{
			GlobalDataMaker.Data().Apply(new DefaultPersonThatCreatesDbData());
			GlobalUnitOfWorkState.UnitOfWorkAction(createLicense);
			GlobalDataMaker.Data().Apply(new DefaultBusinessUnit());
			GlobalDataMaker.Data().Apply(new DefaultScenario());
			GlobalDataMaker.Data().Apply(new DefaultRaptorApplicationFunctions());
			GlobalDataMaker.Data().Apply(new DefaultMatrixApplicationFunctions());
		}

		private static void setupFakeState()
		{
			DefaultBusinessUnit.BusinessUnitFromFakeState = new BusinessUnit("BusinessUnit");

			StateHolderProxyHelper.SetupFakeState(datasource, DefaultPersonThatCreatesDbData.PersonThatCreatesDbData, DefaultBusinessUnit.BusinessUnitFromFakeState, new ThreadPrincipalContext(new TeleoptiPrincipalFactory()));

			GlobalPrincipalState.Principal = Thread.CurrentPrincipal as TeleoptiPrincipal;
			GlobalUnitOfWorkState.CurrentUnitOfWorkFactory = UnitOfWorkFactory.CurrentUnitOfWorkFactory();
		}


		public static void ClearAnalyticsData()
		{
			DataSourceHelper.ClearAnalyticsData();
		}

		private static void backupCcc7Data()
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
	}
}