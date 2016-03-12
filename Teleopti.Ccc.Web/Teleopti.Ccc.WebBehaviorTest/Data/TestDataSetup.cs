using System.Threading;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestDataSetup
	{
		private static IDataSource datasource;
		private static int _dataHash;

		public static void Setup()
		{
			DataSourceHelper.CreateDatabases();

			TestSiteConfigurationSetup.StartApplicationAsync();

			SystemSetup.Setup();

			datasource = DataSourceHelper.CreateDataSource(SystemSetup.PersistCallbacks);

			StateHolderProxyHelper.SetupFakeState(
				datasource,
				DefaultPersonThatCreatesDbData.PersonThatCreatesDbData,
				DefaultBusinessUnit.BusinessUnitFromFakeState,
				new ThreadPrincipalContext()
				);
			GlobalUnitOfWorkState.CurrentUnitOfWorkFactory = UnitOfWorkFactory.CurrentUnitOfWorkFactory();
			GlobalPrincipalState.Principal = Thread.CurrentPrincipal as TeleoptiPrincipal;

			var defaultData = new DefaultData();
			var dataFactory = new DataFactory(action =>
			{
				using (SystemSetup.UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					action.Invoke(SystemSetup.UnitOfWork);
					SystemSetup.UnitOfWork.Current().PersistAll();
				}
			});
			defaultData.ForEach(dataFactory.Apply);

			_dataHash = defaultData.HashValue;
			DataSourceHelper.BackupApplicationDatabase(_dataHash);

			SystemSetup.Start();
		}

		public static void ClearAnalyticsData()
		{
			DataSourceHelper.ClearAnalyticsData();
		}

		public static void RestoreCcc7Data()
		{
			DataSourceHelper.RestoreApplicationDatabase(_dataHash);
		}
	}
}
