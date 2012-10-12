﻿using System;
using System.IO;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
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

				TestSiteConfigurationSetup.Setup();

				log4net.Config.XmlConfigurator.Configure();
				EventualTimeouts.Set(TimeSpan.FromSeconds(10));

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

		[BeforeFeature]
		public static void BeforeFeature()
		{
			if (_scenarioCount > 0)
				Browser.Restart();
		}

		[BeforeScenario]
		public static void BeforeScenario()
		{
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
				Log.Error("Html:");
				Log.Error(Browser.Current.Html);
				Log.Error("Text:");
				Log.Error(Browser.Current.Text);
				//var artifactFileName = GetCommonArtifactFileNameForScenario();
				//SaveScreenshot(artifactFileName);
				//throw MakeScenarioException(artifactFileName);
			}
		}

		//private static ScenarioErrorException MakeScenarioException(string artifact)
		//{
		//    var html = Browser.Current.Html;
		//    var text = Browser.Current.Text;
		//    var message = string.Format("Scenario error occurred.\nArtifact:{0}\n\nText:\n{1}\n\nHtml:\n{2}", artifact, text, html);
		//    return new ScenarioErrorException(message, ScenarioContext.Current.TestError);
		//}

		//private string GetCommonArtifactFileNameForScenario()
		//{
		//    var fileName = ScenarioContext.Current.ScenarioInfo.Title + "." + Path.GetRandomFileName();
		//    fileName = new string(fileName.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());
		//    return Path.Combine(Environment.CurrentDirectory, fileName);
		//}

		//private static void SaveScreenshot(string artifactFileName)
		//{
		//    Browser.Current.CaptureWebPageToFile(artifactFileName + ".jpg");
		//}

	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class ScenarioErrorException : Exception
	{
		public ScenarioErrorException(string message, Exception scenarioError) : base(message, scenarioError) {  }
	}
}