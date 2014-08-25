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

				TestDataSetup.CreateDataSource();

				TestDataSetup.SetupFakeState();
				TestDataSetup.CreateMinimumTestData();

				CreateData();

				TestDataSetup.BackupCcc7Data();

				TestSiteConfigurationSetup.Setup();
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

			addExtraDataSource();
			TestControllerMethods.BeforeScenario();
			
			TestDataSetup.RestoreCcc7Data();
			TestDataSetup.ClearAnalyticsData();


			GlobalPrincipalState.EnsureThreadPrincipal();
			ScenarioUnitOfWorkState.OpenUnitOfWork();

			Log.Debug("Starting scenario " + ScenarioContext.Current.ScenarioInfo.Title);

			checkIfRunTestDueToToggleFlags();
		}

        private static readonly string TargetTestDataNHibFile = Path.Combine(Paths.WebBinPath(), "TestData2.nhib.xml");

        private static void addExtraDataSource()
        {
            if (!ScenarioContext.Current.ScenarioInfo.Tags.Contains("ExtraDataSource")) return;
            FileConfigurator.ConfigureByTags(
                "Data\\TestData2.nhib.xml",
                TargetTestDataNHibFile,
                new AllTags()
                );
        }

	    private static void removeExtraDataSource()
	    {
	        if (!ScenarioContext.Current.ScenarioInfo.Tags.Contains("ExtraDataSource")) return;

	        int i = 0;
	        while (File.Exists(TargetTestDataNHibFile) && i < 20)
	        {
	            i++;
	            File.Delete(TargetTestDataNHibFile);
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
			Log.Debug("Cleaning up after scenario " + ScenarioContext.Current.ScenarioInfo.Title);
            
            Browser.Interactions.GoToWaitForUrlAssert("about:blank", "about:blank");
            
			ScenarioUnitOfWorkState.DisposeUnitOfWork();
			HandleScenarioException();
            removeExtraDataSource();

			Log.Debug("Finished scenario " + ScenarioContext.Current.ScenarioInfo.Title);
		}

		[AfterTestRun]
		public static void AfterTestRun()
		{
			Log.Debug("Cleaing up after test run");

			Browser.Close();
			TestSiteConfigurationSetup.TearDown();

			Log.Debug("Finished test run");
		}

		private static void CreateData()
		{
			GlobalDataMaker.Data().Apply(new CommonBusinessUnit());
			GlobalDataMaker.Data().Apply(new CommonSite());
			GlobalDataMaker.Data().Apply(new CommonScenario());


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
