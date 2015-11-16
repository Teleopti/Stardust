using System;
using System.IO;
using System.Linq;
using log4net;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Toggle;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	public class TestRun
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(TestRun));

		public void Setup()
		{
			log4net.Config.XmlConfigurator.Configure();

			log.Debug("Preparing for test run");

			Browser.SetDefaultTimeouts(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(25));
			TestSiteConfigurationSetup.Setup();
			TestDataSetup.Setup();

			log.Debug("Starting test run");
		}

		public void BeforeScenario()
		{
			log.Debug("Preparing for scenario " + ScenarioContext.Current.ScenarioInfo.Title);
			Browser.SelectBrowserByTag();

			ToggleStepDefinition.CheckIfRunTestDueToToggleFlags();

			CurrentTime.Reset();
			TestControllerMethods.BeforeScenario();

			TestDataSetup.RestoreCcc7Data();
			TestDataSetup.ClearAnalyticsData();

			TestControllerMethods.ClearConnections();

			GlobalPrincipalState.EnsureThreadPrincipal();

			Browser.Interactions.Javascript("sessionStorage.clear();");
			
			log.Debug("Starting scenario " + ScenarioContext.Current.ScenarioInfo.Title);
		}

		public void AfterScenario()
		{
			log.Debug("Cleaning up after scenario " + ScenarioContext.Current.ScenarioInfo.Title);

			Browser.Interactions.GoTo("about:blank");

			ScenarioUnitOfWorkState.TryDisposeUnitOfWork();
			handleScenarioException();

			log.Debug("Finished scenario " + ScenarioContext.Current.ScenarioInfo.Title);
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

			Browser.Close();
			TestSiteConfigurationSetup.TearDown();

			log.Debug("Finished test run");
		}

		private static void handleScenarioException()
		{
			if (ScenarioContext.Current.TestError == null) return;
			Console.WriteLine("\r\nTest Scenario \"{0}\" failed, please check the error message.",
				ScenarioContext.Current.ScenarioInfo.Title);
		}
	}
}