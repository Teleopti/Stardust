﻿using System.Threading;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
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

			DataSourceHelper.BackupApplicationDatabase(SystemSetup.DefaultDataCreator.HashValue);			
		}

		public static void ClearAnalyticsData()
		{
			DataSourceHelper.ClearAnalyticsData();
		}

		public static void RestoreCcc7Data()
		{
			DataSourceHelper.RestoreApplicationDatabase(SystemSetup.DefaultDataCreator.HashValue);
		}
	}
}
