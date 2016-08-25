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

			LocalSystem.Setup();

			datasource = DataSourceHelper.CreateDataSource(LocalSystem.TransactionHooks);

			StateHolderProxyHelper.SetupFakeState(
				datasource,
				DefaultPersonThatCreatesData.PersonThatCreatesDbData,
				DefaultBusinessUnit.BusinessUnit
				);
			GlobalPrincipalState.Principal = Thread.CurrentPrincipal as TeleoptiPrincipal;

			LocalSystem.Start();

			LocalSystem.DefaultDataCreator.Create();
			LocalSystem.DefaultAnalyticsDataCreator.OneTimeSetup();

			DataSourceHelper.BackupApplicationDatabase(LocalSystem.DefaultDataCreator.HashValue);
		}

		public static void ClearAnalyticsData()
		{
			DataSourceHelper.ClearAnalyticsData();
			LocalSystem.DefaultAnalyticsDataCreator.Create();
			TestCommon.TestData.Analytics.BusinessUnit.IdCounter = 0;
		}

		public static void RestoreCcc7Data()
		{
			DataSourceHelper.RestoreApplicationDatabase(LocalSystem.DefaultDataCreator.HashValue);
		}

		public static void SetupDefaultScenario()
		{
			StateHolderProxyHelper.SetupFakeState(
				datasource,
				DefaultPersonThatCreatesData.PersonThatCreatesDbData,
				DefaultBusinessUnit.BusinessUnit
				);

			LocalSystem.DefaultDataCreator.CreateDefaultScenario();
		}
	}
}
