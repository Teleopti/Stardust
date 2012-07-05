using System;
using System.IO;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User;
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
			Browser.PrepareForTestRun();

			try
			{
				if (!Browser.IsStarted())
					Browser.Start();

				TestControllerMethods.BeforeTestRun();

				TestSiteConfigurationSetup.Setup();

				log4net.Config.XmlConfigurator.Configure();
				EventualTimeouts.Set(TimeSpan.FromSeconds(5));

				TestDataSetup.CreateDataSource();

				TestDataSetup.SetupFakeState();
				TestDataSetup.CreateMinimumTestData();
				CreateData();

				TestDataSetup.BackupCcc7Data();
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
			TestDataSetup.RestoreCcc7Data();
			TestDataSetup.ClearAnalyticsData();
		}

		[AfterScenario]
		public void AfterScenario()
		{
			HandleScenarioException();
			TestControllerMethods.AfterScenario();
		}

		[AfterTestRun]
		public static void AfterTestRun()
		{
			if (Browser.IsStarted())
				Browser.Close();
			TestSiteConfigurationSetup.TearDown();
		}


		private static void CreateData()
		{
			TestDataSetup.CreateLegacyTestData();

			DataContext.Data().Setup(new CommonContract());
			DataContext.Data().Setup(new ContractScheduleWith2DaysOff());
			DataContext.Data().Setup(new CommonScenario());
			DataContext.Data().Setup(new SecondScenario());
			DataContext.Data().Persist();
		}



		private void HandleScenarioException()
		{
			if (!Browser.IsStarted()) return;
			if (ScenarioContext.Current.TestError != null)
			{
				var artifactFileName = GetCommonArtifactFileNameForScenario();
				SaveScreenshot(artifactFileName);
				throw MakeScenarioException(artifactFileName);
			}
		}

		private static ScenarioErrorException MakeScenarioException(string artifact)
		{
			var html = Browser.Current.Html;
			var text = Browser.Current.Text;
			var message = string.Format("Scenario error occurred.\nArtifact:{0}\n\nText:\n{1}\n\nHtml:\n{2}", artifact, text, html);
			return new ScenarioErrorException(message, ScenarioContext.Current.TestError);
		}

		private string GetCommonArtifactFileNameForScenario()
		{
			var fileName = ScenarioContext.Current.ScenarioInfo.Title + "." + Path.GetRandomFileName();
			fileName = new string(fileName.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());
			return Path.Combine(Environment.CurrentDirectory, fileName);
		}

		private static void SaveScreenshot(string artifactFileName)
		{
			Browser.Current.CaptureWebPageToFile(artifactFileName + ".jpg");
		}

	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class ScenarioErrorException : Exception
	{
		public ScenarioErrorException(string message, Exception scenarioError) : base(message, scenarioError) {  }
	}
}