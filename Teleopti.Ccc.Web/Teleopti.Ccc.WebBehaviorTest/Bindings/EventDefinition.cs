﻿using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class EventDefinition
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(EventDefinition));

		[BeforeTestRun]
		public static void BeforeTestRun()
		{
			Log.Debug("Preparing for test run");

			Browser.NotifyBeforeTestRun();

			try
			{
				if (!Browser.IsStarted())
					Browser.Start(TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(25));

				TestControllerMethods.BeforeTestRun();

				TestSiteConfigurationSetup.RecycleApplication();

				log4net.Config.XmlConfigurator.Configure();

				TestDataSetup.CreateDataSource();

				TestDataSetup.SetupFakeState();
				TestDataSetup.CreateMinimumTestData();
				CreateData();

				TestDataSetup.BackupCcc7Data();

				TestSiteConfigurationSetup.Setup();

				ServiceBusSetup.Setup();
			}
			catch (Exception)
			{
				Browser.Close();
				throw;
			}
			finally
			{
				Log.Debug("Starting test run");
			}
		}

		[BeforeScenario]
		public static void BeforeScenario()
		{
			Log.Debug("Preparing for scenario " + ScenarioContext.Current.ScenarioInfo.Title);

			Browser.NotifyBeforeScenario();
			
			TestControllerMethods.BeforeScenario();
			
			TestDataSetup.RestoreCcc7Data();
			TestDataSetup.ClearAnalyticsData();

			GlobalPrincipalState.EnsureThreadPrincipal();
			ScenarioUnitOfWorkState.OpenUnitOfWork();

			Log.Debug("Starting scenario " + ScenarioContext.Current.ScenarioInfo.Title);
		}

		[AfterScenario]
		public void AfterScenario()
		{
			Log.Debug("Cleaning up after scenario " + ScenarioContext.Current.ScenarioInfo.Title);

			ScenarioUnitOfWorkState.DisposeUnitOfWork();
			HandleScenarioException();

			Log.Debug("Finished scenario " + ScenarioContext.Current.ScenarioInfo.Title);
		}

		[AfterTestRun]
		public static void AfterTestRun()
		{
			Log.Debug("Cleaing up after test run");

			if (Browser.IsStarted())
				Browser.Close();
			ServiceBusSetup.TearDown();
			TestSiteConfigurationSetup.TearDown();

			Log.Debug("Finished test run");
		}

		private static void CreateData()
		{
			GlobalDataContext.Data().Setup(new CommonBusinessUnit());
			GlobalDataContext.Data().Setup(new CommonSite());
			GlobalDataContext.Data().Setup(new CommonTeam());
			GlobalDataContext.Data().Setup(new CommonScenario());

			GlobalDataContext.Data().Setup(new CommonPartTimePercentage());
			GlobalDataContext.Data().Setup(new CommonContract());
			GlobalDataContext.Data().Setup(new CommonContractSchedule());

			GlobalDataContext.Data().Setup(new SecondBusinessUnit());
			GlobalDataContext.Data().Setup(new AnotherSite());
			GlobalDataContext.Data().Setup(new SecondScenario());

			GlobalDataContext.Persist();

			TestDataSetup.CreateLegacyTestData();
		}



		private void HandleScenarioException()
		{
			if (!Browser.IsStarted()) return;
			if (ScenarioContext.Current.TestError != null)
			{
				Log.Error("Scenario exception occurred, dumping info here.");
				Browser.Interactions.DumpInfo(Log.Error);
			}
		}

	}

}