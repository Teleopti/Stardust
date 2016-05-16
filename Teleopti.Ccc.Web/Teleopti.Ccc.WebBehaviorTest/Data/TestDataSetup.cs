using System.Threading;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestDataSetup
	{
		private static IDataSource datasource;

		public static void Setup()
		{
			DataSourceHelper.CreateDatabases();

			TestSiteConfigurationSetup.StartApplicationAsync();

			SystemSetup.Setup();

			datasource = DataSourceHelper.CreateDataSource(SystemSetup.TransactionHooks);

			StateHolderProxyHelper.SetupFakeState(
				datasource,
				DefaultPersonThatCreatesData.PersonThatCreatesDbData,
				DefaultBusinessUnit.BusinessUnit,
				new ThreadPrincipalContext()
				);
			GlobalPrincipalState.Principal = Thread.CurrentPrincipal as TeleoptiPrincipal;

			SystemSetup.Start();

			SystemSetup.DefaultDataCreator.Create();
			SystemSetup.DefaultAnalyticsDataCreator.Create();

			DataSourceHelper.BackupApplicationDatabase(SystemSetup.DefaultDataCreator.HashValue);			
			DataSourceHelper.BackupAnalyticsDatabase(SystemSetup.DefaultAnalyticsDataCreator.HashValue);			
		}

		public static void ClearAnalyticsData()
		{
			DataSourceHelper.ClearAnalyticsData();
		}

		public static void RestoreAnalyticsData()
		{
			DataSourceHelper.RestoreAnalyticsDatabase(SystemSetup.DefaultAnalyticsDataCreator.HashValue);
			TestCommon.TestData.Analytics.BusinessUnit.IdCounter = 0;
		}
		public static void RestoreCcc7Data()
		{
			DataSourceHelper.RestoreApplicationDatabase(SystemSetup.DefaultDataCreator.HashValue);
		}
	}
}
