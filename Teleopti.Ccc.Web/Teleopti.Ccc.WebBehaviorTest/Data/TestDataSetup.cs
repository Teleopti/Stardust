using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

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

			datasource = DataSourceHelper.CreateDataSource(IntegrationIoCTest.Container);

			StateHolderProxyHelper.SetupFakeState(
				datasource,
				DefaultPersonThatCreatesData.PersonThatCreatesDbData,
				DefaultBusinessUnit.BusinessUnit
				);
			GlobalPrincipalState.Principal = Thread.CurrentPrincipal as TeleoptiPrincipalForLegacy;

			LocalSystem.Start();

			LocalSystem.DefaultDataCreator.Create();
			LocalSystem.DefaultAnalyticsDataCreator.OneTimeSetup();

			DataSourceHelper.BackupApplicationDatabase(DefaultDataCreator.HashValue);
		}

		public static void ClearAnalyticsData()
		{
			DataSourceHelper.ClearAnalyticsData();
			LocalSystem.DefaultAnalyticsDataCreator.Create();
			BusinessUnit.IdCounter = 0;
			SpecificTimeZone.TimeZones.Clear();
		}

		public static void RestoreCcc7Data()
		{
			DataSourceHelper.RestoreApplicationDatabase(DefaultDataCreator.HashValue);
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

		public static void CreateFirstTenantAdminUser()
		{
			DataSourceHelper.CreateFirstTenantAdminUser();
		}
	}
}
