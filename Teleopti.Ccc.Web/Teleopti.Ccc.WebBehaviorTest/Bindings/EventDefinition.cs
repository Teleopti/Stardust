using System;
using System.IO;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using log4net;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;
using Teleopti.Ccc.WebBehaviorTest.Toggle;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	public class BeforeTestRunException : Exception
	{
		public BeforeTestRunException(Exception innerException)
			: base("Exception occurred in BeforeTestRun", innerException)
		{
		}
	}

	[Binding]
	public class EventDefinition
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(EventDefinition));
		private static Exception _beforeTestRunException;

		// THIS METHOD WILL BE RETRIED WHEN FAILED!
		// So we catch all exceptions...
		// .. so that it wont be retried
		// .. and rethrow in BeforeScenario so all tests will fail with the same exception
		// .. and dont run AfterScenario at all
		// .. and let AfterTestRun clean up
		[BeforeTestRun]
		public static void BeforeTestRun()
		{
			try
			{
				log4net.Config.XmlConfigurator.Configure();

				log.Debug("Preparing for test run");

				Browser.SetDefaultTimeouts(TimeSpan.FromSeconds(20), TimeSpan.FromMilliseconds(25));
				TestSiteConfigurationSetup.Setup();
				TestDataSetup.CreateDataSourceAndStartWebApp();

				log.Debug("Starting test run");
			}
			catch (Exception e)
			{
				_beforeTestRunException = e;
			}
		}
		
		[BeforeScenario]
		public static void BeforeScenario()
		{
			if (_beforeTestRunException != null)
				throw new BeforeTestRunException(_beforeTestRunException);

			log.Debug("Preparing for scenario " + ScenarioContext.Current.ScenarioInfo.Title);
			Browser.SelectBrowserByTag();

			ToggleStepDefinition.CheckIfRunTestDueToToggleFlags();

			addExtraDataSource();
			CurrentTime.Reset();
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

		[AfterScenario]
		public static void AfterScenario()
		{
			if (_beforeTestRunException != null)
				return;

			log.Debug("Cleaning up after scenario " + ScenarioContext.Current.ScenarioInfo.Title);
            
            Browser.Interactions.GoTo("about:blank");
            
			ScenarioUnitOfWorkState.TryDisposeUnitOfWork();
			handleScenarioException();
			File.Delete(targetTestDataNHibFile);

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
