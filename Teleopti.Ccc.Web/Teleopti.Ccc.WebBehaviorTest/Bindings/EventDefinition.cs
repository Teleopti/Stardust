using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using log4net;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;
using Teleopti.Ccc.WebBehaviorTest.Toggle;

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
			ToggleStepDefinition.CheckIfRunTestDueToToggleFlags();

			Browser.SelectBrowserByTag();

			addExtraDataSource();
			TestControllerMethods.BeforeScenario();
			
			TestDataSetup.RestoreCcc7Data();
			TestDataSetup.ClearAnalyticsData();
			shouldBeDeleted();

			GlobalPrincipalState.EnsureThreadPrincipal();

			log.Debug("Starting scenario " + ScenarioContext.Current.ScenarioInfo.Title);
		}

		private static void shouldBeDeleted()
		{
			//hack to reset global #¤%#¤% scenario state to db state.
			//"DefaultScenario" should be deleted!
			GlobalUnitOfWorkState.UnitOfWorkAction(uow => DefaultScenario.Scenario = new ScenarioRepository(uow).LoadDefaultScenario());
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
		
		[AfterScenario]
		public static void AfterScenario()
		{
			log.Debug("Cleaning up after scenario " + ScenarioContext.Current.ScenarioInfo.Title);
            
            Browser.Interactions.GoToWaitForUrlAssert("about:blank", "about:blank");
            
			ScenarioUnitOfWorkState.TryDisposeUnitOfWork();
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
