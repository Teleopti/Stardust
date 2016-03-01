using System.Threading;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.TestData
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
			GlobalPrincipalState.Principal = Thread.CurrentPrincipal as TeleoptiPrincipal;

			var defaultData = new DefaultData();
			new WithUnitOfWork(CurrentUnitOfWorkFactory.Make()).Do(() =>
			{
				var dataFactory = new DataFactory(CurrentUnitOfWork.Make());
				defaultData.ForEach(dataSetup => dataFactory.Apply(dataSetup));
			});

			_dataHash = defaultData.HashValue;
			DataSourceHelper.BackupCcc7Database(_dataHash);

			SystemSetup.Start();
		}

		public static void ClearAnalyticsData()
		{
			DataSourceHelper.ClearAnalyticsData();
		}

		public static void RestoreCcc7Data()
		{
			DataSourceHelper.RestoreCcc7Database(_dataHash);
		}
	}
}
