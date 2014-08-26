using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
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
		private static IDataSource datasource;
		private static readonly DefaultData globalData = new DefaultData();

		public static void CreateDataSource()
		{
			if (!DataSourceHelper.Ccc7BackupExists(globalData.HashValue))
			{
				datasource = DataSourceHelper.CreateDataSourceNoBackup(new[]
				{
					new EventsMessageSender(
						new SyncEventsPublisher(new EventPublisher(new HardCodedResolver(),
							new EventContextPopulator(new CurrentIdentity(), new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make())))))

				}, true);
				setupFakeState();
				createGlobalDbData();
				backupCcc7Data();
			}
			else
			{
				datasource = DataSourceHelper.CreateDataSourceNoBackup(new[]
				{
					new EventsMessageSender(
						new SyncEventsPublisher(new EventPublisher(new HardCodedResolver(),
							new EventContextPopulator(new CurrentIdentity(), new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make())))))

				}, false);
			}
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
			DataSourceHelper.BackupCcc7Database(globalData.HashValue);
		}

		public static void RestoreCcc7Data()
		{
			DataSourceHelper.RestoreCcc7Database(globalData.HashValue, () => Navigation.GoToWaitForUrlAssert("Test/ClearConnections", new ApplicationStartupTimeout()));
			//hack!
			using (var uow = ((NHibernateUnitOfWorkFactory)datasource.Application).CreateAndOpenUnitOfWork(null, TransactionIsolationLevel.Default, null))
			{
				DefaultPersonThatCreatesDbData.PersonThatCreatesDbData = new PersonRepository(uow).LoadAll().Single(x => x.Name == DefaultPersonThatCreatesDbData.PersonThatCreatesDbData.Name);
				DefaultBusinessUnit.BusinessUnitFromFakeState = new BusinessUnitRepository(uow).LoadAll().Single(x => x.Name == DefaultBusinessUnit.BusinessUnitFromFakeState.Name);
				setupFakeState();
			}
		}
	}
}