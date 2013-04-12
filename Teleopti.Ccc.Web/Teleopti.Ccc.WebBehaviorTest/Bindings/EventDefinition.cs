using System;
using System.IO;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
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

		private static int _scenarioCount = 0;

		private static void ResetScenarioCount() { _scenarioCount = 0; }
		private static void IncrementScenarioCount() { _scenarioCount += 1; }

		[BeforeTestRun]
		public static void BeforeTestRun()
		{
			ResetScenarioCount();

			Browser.PrepareForTestRun();

			try
			{
				if (!Browser.IsStarted())
					Browser.Start();

				TestControllerMethods.BeforeTestRun();

				TestSiteConfigurationSetup.RecycleApplication();

				log4net.Config.XmlConfigurator.Configure();
				Timeouts.Set(TimeSpan.FromSeconds(30));

				TestDataSetup.CreateDataSource();

				TestDataSetup.SetupFakeState();
				TestDataSetup.CreateMinimumTestData();
				CreateData();

				TestDataSetup.BackupCcc7Data();

				TestSiteConfigurationSetup.Setup();

				ServiceBusSetup.Setup();
			}
			catch(Exception)
			{
				Browser.Close();
				throw;
			}
		}

		[BeforeScenario]
		public static void BeforeScenario()
		{
			// restart browser every 20th scenario
			if (_scenarioCount != 0 && _scenarioCount % 15 == 0)
				Browser.Restart();

			IncrementScenarioCount();

			TestControllerMethods.BeforeScenario();
			
			TestDataSetup.RestoreCcc7Data();
			TestDataSetup.ClearAnalyticsData();

			GlobalPrincipalState.EnsureThreadPrincipal();
			ScenarioUnitOfWorkState.OpenUnitOfWork();
		}

		[AfterScenario]
		public void AfterScenario()
		{
			ScenarioUnitOfWorkState.DisposeUnitOfWork();
			HandleScenarioException();
		}

		[AfterTestRun]
		public static void AfterTestRun()
		{
			if (Browser.IsStarted())
				Browser.Close();
			ServiceBusSetup.TearDown();
			TestSiteConfigurationSetup.TearDown();
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
				Log.Error("Scenario exception occurred, dumping text and html.");
				var html = "";
				var text = "";
				try
				{
					html = Browser.Current.Html;
					text = Browser.Current.Text;
				} catch(Exception ex)
				{
					html = ex.ToString();
					text = ex.ToString();
				}
				Log.Error("Html:");
				Log.Error(html);
				Log.Error("Text:");
				Log.Error(text);
			}
		}

	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class ScenarioErrorException : Exception
	{
		public ScenarioErrorException(string message, Exception scenarioError) : base(message, scenarioError) {  }
	}
}