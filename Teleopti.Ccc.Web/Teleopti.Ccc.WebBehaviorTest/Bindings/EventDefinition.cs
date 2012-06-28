using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
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
		private static DateTime _testRunStartTime;
		private static DateTime _testRunScenariosStartTime;
		private static TimeSpan _beforeScenarioTimeSum;
		private static int _scenarioCount;

		[BeforeTestRun]
		public static void BeforeTestRun()
		{
			var startTime = DateTime.Now;
			_testRunStartTime = DateTime.Now;
			_beforeScenarioTimeSum = TimeSpan.Zero;
			_scenarioCount = 0;

			log4net.Config.XmlConfigurator.Configure();
			Browser.PrepareForTestRun();
			EventualTimeouts.Set(TimeSpan.FromSeconds(5));

			TestDataSetup.CreateDataSource();
			TestSiteConfigurationSetup.Setup();

			PrepareData();

			Log.Write("BeforeTestRun took " + DateTime.Now.Subtract(startTime));
			_testRunScenariosStartTime = DateTime.Now;
		}

		[BeforeScenario]
		public static void BeforeScenario()
		{
			_scenarioCount++;
			var startTime = DateTime.Now;

			//SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
			//var prepareData = Task.Factory.StartNew(PrepareData);
			//var startBrowser = Task.Factory.StartNew(() =>
			//                                            {
			//                                                var x = Browser.Current.hWnd;
			//                                            },
			//                                            CancellationToken.None, 
			//                                            TaskCreationOptions.None, 
			//                                            TaskScheduler.FromCurrentSynchronizationContext());
			//Task.WaitAll(prepareData, startBrowser);

			//PrepareData();

			var spentTime = DateTime.Now.Subtract(startTime);
			Log.Write("BeforeScenario took " + spentTime);
			_beforeScenarioTimeSum = _beforeScenarioTimeSum.Add(spentTime);
		}

		[AfterScenario]
		public void AfterScenario()
		{
			var startTime = DateTime.Now;

			CloseBrowserAndSaveExceptions();

			Log.Write("AfterScenario took " + DateTime.Now.Subtract(startTime));
			Log.Write("Run as taken " + DateTime.Now.Subtract(_testRunStartTime));
			Log.Write("Scenario time sum " + DateTime.Now.Subtract(_testRunScenariosStartTime));
			Log.Write("BeforeScenario sum " + _beforeScenarioTimeSum);
			Log.Write("BeforeScenario time/scenario" + TimeSpan.FromMilliseconds(_beforeScenarioTimeSum.TotalMilliseconds / _scenarioCount));
		}

		[AfterTestRun]
		public static void AfterTestRun()
		{
			TestSiteConfigurationSetup.TearDown();
		}




		private static void PrepareData()
		{
			//ITeleoptiPrincipal principal = null;
			//var clearCcc7Data = Task.Factory.StartNew(TestDataSetup.ClearCcc7Data);
			//var clearAnalyticsData = Task.Factory.StartNew(TestDataSetup.ClearAnalyticsData);
			//var setupFakeState = Task.Factory.StartNew(() =>
			//                                            {
			//                                                TestDataSetup.SetupFakeState();
			//                                                principal = Thread.CurrentPrincipal as ITeleoptiPrincipal;
			//                                            });
			//var createTestData = Task.Factory.StartNew(() =>
			//                                            {
			//                                                Task.WaitAll(clearCcc7Data, setupFakeState);
			//                                                Thread.CurrentPrincipal = principal;

			//                                                TestDataSetup.CreateLegacyTestData();

			//                                                DataContext.Data().Setup(new CommonContract());
			//                                                DataContext.Data().Setup(new ContractScheduleWith2DaysOff());
			//                                                DataContext.Data().Setup(new CommonScenario());
			//                                                DataContext.Data().Setup(new SecondScenario());
			//                                                DataContext.Data().Persist();

			//                                            });

			//Task.WaitAll(clearCcc7Data, clearAnalyticsData, setupFakeState, createTestData);
			//Thread.CurrentPrincipal = principal;
			//return;

			TestDataSetup.SetupFakeState();

			TestDataSetup.ClearCcc7Data();
			TestDataSetup.CreateLegacyTestData();

			DataContext.Data().Setup(new CommonContract());
			DataContext.Data().Setup(new ContractScheduleWith2DaysOff());
			DataContext.Data().Setup(new CommonScenario());
			DataContext.Data().Setup(new SecondScenario());
			DataContext.Data().Persist();

			TestDataSetup.ClearAnalyticsData();
		}


		private void CloseBrowserAndSaveExceptions()
		{
			if (!Browser.IsStarted()) return;
			try
			{
				if (ScenarioContext.Current.TestError != null)
				{
					var artifactFileName = GetCommonArtifactFileNameForScenario();
					SaveScreenshot(artifactFileName);
					throw MakeScenarioException(artifactFileName);
				}
			}
			finally
			{
				Browser.Close();
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