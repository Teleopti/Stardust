using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestDataSetup
	{
		private static DatabaseHelper.Backup _ccc7DataBackup;
		private static IDataSource datasource;
		private static readonly IEnumerable<IHashableDataSetup> globalData = new IHashableDataSetup[]
		{
			new DefaultPersonThatCreatesDbData(),
			new DefaultLicense(),
			new DefaultBusinessUnit(),
			new DefaultScenario(),
			new DefaultRaptorApplicationFunctions(),
			new DefaultMatrixApplicationFunctions()
		};

		public static void CreateDataSource()
		{
			datasource = DataSourceHelper.CreateDataSource(new[] { new EventsMessageSender(new SyncEventsPublisher(new EventPublisher(new HardCodedResolver(), new EventContextPopulator(new CurrentIdentity(), new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make()))))) }, "TestData");

			setupFakeState();
			createGlobalDbData();

			backupCcc7Data();
		}

		private static void createGlobalDbData()
		{
			globalData.ForEach(dataSetup => GlobalDataMaker.Data().Apply(dataSetup));
		}

		private static void setupFakeState()
		{
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
	}
}