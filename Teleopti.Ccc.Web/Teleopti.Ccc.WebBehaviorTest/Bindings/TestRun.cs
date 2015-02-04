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
		private static readonly ILog log = LogManager.GetLogger(typeof(EventBindings));

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

			addExtraDataSource();
			CurrentTime.Reset();
			TestControllerMethods.BeforeScenario();

			TestDataSetup.RestoreCcc7Data();
			TestDataSetup.ClearAnalyticsData();

			GlobalPrincipalState.EnsureThreadPrincipal();

			log.Debug("Starting scenario " + ScenarioContext.Current.ScenarioInfo.Title);
		}

		private static readonly string targetTestDataNHibFile = Path.Combine(Paths.WebBinPath(), "TestData2.nhib.xml");
		private void addExtraDataSource()
		{
			if (!ScenarioContext.Current.ScenarioInfo.Tags.Contains("ExtraDataSource")) return;
			FileConfigurator.ConfigureByTags(
				"Data\\TestData2.nhib.xml",
				targetTestDataNHibFile,
				new AllTags()
				);
		}

		public void AfterScenario()
		{
			log.Debug("Cleaning up after scenario " + ScenarioContext.Current.ScenarioInfo.Title);

			Browser.Interactions.GoTo("about:blank");

			ScenarioUnitOfWorkState.TryDisposeUnitOfWork();
			handleScenarioException();
			File.Delete(targetTestDataNHibFile);

			log.Debug("Finished scenario " + ScenarioContext.Current.ScenarioInfo.Title);
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

			log.Error("Scenario exception occurred, dumping info here.");

			Browser.Interactions.DumpInfo(log.Error);
		}

	}
}