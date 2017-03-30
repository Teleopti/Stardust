using System;
using System.IO;
using log4net;
using log4net.Config;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Toggle;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	public class TestRun
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(TestRun));
		private const string suppressHangfireQueueTag = "suppressHangfireQueue";

		public void Setup()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			XmlConfigurator.Configure();

			log.Debug("Preparing for test run");

			Browser.SetDefaultTimeouts(TimeSpan.FromSeconds(20), TimeSpan.FromMilliseconds(25));
			TestSiteConfigurationSetup.Setup();
			TestDataSetup.Setup();

			log.Debug("Starting test run");
		}

		public void BeforeScenario()
		{
			log.Debug($"Preparing for scenario {ScenarioContext.Current.ScenarioInfo.Title}");
			
			Browser.SelectBrowserByTag();

			ToggleStepDefinition.IgnoreScenarioIfDisabledByToggle();

			CurrentTime.Reset();
			CurrentScopeBusinessUnit.Reset();
			TestControllerMethods.BeforeScenario();

			TestDataSetup.RestoreCcc7Data();
			TestDataSetup.ClearAnalyticsData();
			TestDataSetup.SetupDefaultScenario();
			TestDataSetup.CreateFirstTenantAdminUser();

			TestControllerMethods.ClearConnections();

			GlobalPrincipalState.EnsureThreadPrincipal();

			Browser.Interactions.Javascript("sessionStorage.clear();");
			
			log.Debug($"Starting scenario {ScenarioContext.Current.ScenarioInfo.Title}");
		}

		public void AfterScenario()
		{
			log.Debug($"Cleaning up after scenario {ScenarioContext.Current.ScenarioInfo.Title}");
			log.Info(TestContext.CurrentContext.Result.Outcome.Status);
			if (Browser.IsStarted)
				Browser.Interactions.GoTo("about:blank");
			DataMaker.AfterScenario();
			// some scenarios should not trigger handfire queue jobs which may throw an exception caused by no data available.
			if (!suppressHangfireQueue())
			{
				LocalSystem.Hangfire.WaitForQueue();
			}

			if (ScenarioContext.Current.TestError != null)
				Console.WriteLine($"\r\nTest Scenario \"{ScenarioContext.Current.ScenarioInfo.Title}\" failed, please check the error message.");

			log.Debug($"Finished scenario {ScenarioContext.Current.ScenarioInfo.Title}");
		}

		public void AfterStep()
		{
			if (ScenarioContext.Current.TestError == null) return;
			log.Error("Step exception occurred, dumping info here.");

			Browser.Interactions.DumpInfo(log.Error);
		}

		public void TearDown()
		{
			log.Debug("Cleaing up after test run");

			Browser.Dispose();
			TestSiteConfigurationSetup.TearDown();

			log.Debug("Finished test run");
		}

		private static bool suppressHangfireQueue()
		{
			return ScenarioContext.Current.IsTaggedWith(suppressHangfireQueueTag);
		}
	}
}