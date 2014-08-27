using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class EventDefinition
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(EventDefinition));

		[BeforeTestRun]
		public static void BeforeTestRun()
		{
			log4net.Config.XmlConfigurator.Configure();

			log.Debug("Preparing for test run");

			Browser.SelectDefaultVisibleBrowser();
			try
			{
				Browser.SetDefaultTimeouts(TimeSpan.FromSeconds(20), TimeSpan.FromMilliseconds(25));
				TestSiteConfigurationSetup.Setup();
				TestDataSetup.CreateDataSourceAndStartWebApp();
			}
			catch (Exception)
			{
				Browser.Close();
				throw;
			}
			finally
			{
				log.Debug("Starting test run");
			}
		}
		
		[BeforeScenario]
		public static void BeforeScenario()
		{
			log.Debug("Preparing for scenario " + ScenarioContext.Current.ScenarioInfo.Title);

			Browser.SelectBrowserByTag();

			addExtraDataSource();
			TestControllerMethods.BeforeScenario();
			
			TestDataSetup.RestoreCcc7Data();
			TestDataSetup.ClearAnalyticsData();

			GlobalPrincipalState.EnsureThreadPrincipal();

			log.Debug("Starting scenario " + ScenarioContext.Current.ScenarioInfo.Title);

			checkIfRunTestDueToToggleFlags();
		}

        private static readonly string targetTestDataNHibFile = Path.Combine(Paths.WebBinPath(), "TestData2.nhib.xml");

        private static void addExtraDataSource()
        {
            if (!ScenarioContext.Current.ScenarioInfo.Tags.Contains("ExtraDataSource")) return;
            FileConfigurator.ConfigureByTags(
                "Data\\TestData2.nhib.xml",
                targetTestDataNHibFile,
                new AllTags()
                );
        }

	    private static void removeExtraDataSource()
	    {
	        if (!ScenarioContext.Current.ScenarioInfo.Tags.Contains("ExtraDataSource")) return;

	        int i = 0;
	        while (File.Exists(targetTestDataNHibFile) && i < 20)
	        {
	            i++;
	            File.Delete(targetTestDataNHibFile);
	        }
	    }

	    private static void checkIfRunTestDueToToggleFlags()
	    {
				const string ignoreMessage = "Ignore toggle {0} because it is {1}.";

				var toggleQuerier = new ToggleQuerier(TestSiteConfigurationSetup.URL.ToString());
				var matchingEnkelsnuffs = new Regex(@"\'(.*)\'");
		    var tags = ScenarioContext.Current.ScenarioInfo.Tags.Union(FeatureContext.Current.FeatureInfo.Tags).ToArray();

		    var allOnlyRunIfEnabled = tags.Where(s => s.StartsWith("OnlyRunIfEnabled"))
					.Select(onlyRunIfEnabled => (Toggles)Enum.Parse(typeof(Toggles), matchingEnkelsnuffs.Match(onlyRunIfEnabled).Groups[1].ToString()));
		    var allOnlyRunIfDisabled = tags.Where(s => s.StartsWith("OnlyRunIfDisabled"))
					.Select(onlyRunIfDisabled => (Toggles)Enum.Parse(typeof(Toggles), matchingEnkelsnuffs.Match(onlyRunIfDisabled).Groups[1].ToString()));

		    foreach (var toggleOnlyRunIfDisabled in allOnlyRunIfDisabled.Where(toggleQuerier.IsEnabled))
		    {
			    Assert.Ignore(ignoreMessage, toggleOnlyRunIfDisabled, "enabled");
		    }

		    foreach (var toggleOnlyRunIfEnabled in allOnlyRunIfEnabled.Where(toggleOnlyRunIfEnabled => !toggleQuerier.IsEnabled(toggleOnlyRunIfEnabled)))
		    {
			    Assert.Ignore(ignoreMessage, toggleOnlyRunIfEnabled, "disabled");
		    }
	    }
		
		[AfterScenario]
		public static void AfterScenario()
		{
			log.Debug("Cleaning up after scenario " + ScenarioContext.Current.ScenarioInfo.Title);
            
            Browser.Interactions.GoToWaitForUrlAssert("about:blank", "about:blank");
            
			ScenarioUnitOfWorkState.DisposeUnitOfWork();
			handleScenarioException();
            removeExtraDataSource();

			log.Debug("Finished scenario " + ScenarioContext.Current.ScenarioInfo.Title);
		}

		[AfterTestRun]
		public static void AfterTestRun()
		{
			log.Debug("Cleaing up after test run");

			Browser.Close();
			TestSiteConfigurationSetup.TearDown();

			log.Debug("Finished test run");
		}

		private static void handleScenarioException()
		{
			if (ScenarioContext.Current.TestError != null)
			{
				log.Error("Scenario exception occurred, dumping info here.");
				Browser.Interactions.DumpInfo(log.Error);
			}
		}
	}
}
