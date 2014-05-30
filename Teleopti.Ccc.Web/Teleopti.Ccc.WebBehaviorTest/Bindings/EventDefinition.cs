﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;
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
			log4net.Config.XmlConfigurator.Configure();

			Log.Debug("Preparing for test run");

			Browser.SelectDefaultVisibleBrowser();
			try
			{
				Browser.SetDefaultTimeouts(TimeSpan.FromSeconds(20), TimeSpan.FromMilliseconds(25));

				TestControllerMethods.BeforeTestRun();

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

			Browser.SelectBrowserByTag();
			Browser.NotifyBeforeScenario();
			
			TestControllerMethods.BeforeScenario();
			
			TestDataSetup.RestoreCcc7Data();
			TestDataSetup.ClearAnalyticsData();

			GlobalPrincipalState.EnsureThreadPrincipal();
			ScenarioUnitOfWorkState.OpenUnitOfWork();

			Log.Debug("Starting scenario " + ScenarioContext.Current.ScenarioInfo.Title);
			
			if (shouldIgnoreTest("OnlyRunIfDisabled", false) || shouldIgnoreTest("OnlyRunIfEnabled", true))
			{
				Assert.Ignore("Ignored because of featureflags");
			}
		}

		private static bool shouldIgnoreTest(string featureName, bool shouldBeEnabled)
		{
			var scenario = ScenarioContext.Current.ScenarioInfo.Tags.Where(s => s.StartsWith(featureName));
			var regex = new Regex(@"\(.*\)");
			foreach (var tags in scenario)
			{
				var toggleQuerier1 = new ToggleQuerier(new Uri(TestSiteConfigurationSetup.Url, "ToggleHandler/IsEnabled").ToString());
				var foo = regex.Match(tags).ToString();
				foo = foo.Substring(2, foo.Length - 4);
				var isEnabled = toggleQuerier1.IsEnabled((Toggles)Enum.Parse(typeof(Toggles), foo));

				return shouldBeEnabled ? !isEnabled : isEnabled;
			}
			return false;
		}
		
		[AfterScenario]
		public static void AfterScenario()
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

			Browser.Close();
			ServiceBusSetup.TearDown();
			TestSiteConfigurationSetup.TearDown();

			Log.Debug("Finished test run");
		}

		private static void CreateData()
		{
			GlobalDataMaker.Data().Apply(new CommonBusinessUnit());
			GlobalDataMaker.Data().Apply(new CommonSite());
			GlobalDataMaker.Data().Apply(new CommonTeam());
			GlobalDataMaker.Data().Apply(new CommonScenario());

			GlobalDataMaker.Data().Apply(new CommonPartTimePercentage());
			GlobalDataMaker.Data().Apply(new CommonContract());
			GlobalDataMaker.Data().Apply(new CommonContractSchedule());

			GlobalDataMaker.Data().Apply(new SecondBusinessUnit());
			GlobalDataMaker.Data().Apply(new AnotherSite());
			GlobalDataMaker.Data().Apply(new SecondScenario());

			TestDataSetup.CreateLegacyTestData();
		}



		private static void HandleScenarioException()
		{
			if (ScenarioContext.Current.TestError != null)
			{
				Log.Error("Scenario exception occurred, dumping info here.");
				Browser.Interactions.DumpInfo(Log.Error);
			}
		}

	}
}
