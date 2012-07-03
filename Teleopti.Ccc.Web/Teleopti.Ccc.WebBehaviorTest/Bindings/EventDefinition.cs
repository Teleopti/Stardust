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
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	public static class ExperimentalDataMode
	{
		public static bool ForEachScenario = true;
	}

	[Binding]
	public class EventDefinition
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(EventDefinition));

		private static DateTime _testRunStartTime;
		private static DateTime _testRunScenariosStartTime;
		private static TimeSpan _beforeScenarioTimeSpent;
		private static TimeSpan _afterScenarioTimeSpent;
		private static TimeSpan _clearDataTimeSpent;
		private static TimeSpan _createDataTimeSpent;
		private static int _scenarioCount;

		[BeforeTestRun]
		public static void BeforeTestRun()
		{
			var startTime = DateTime.Now;
			_testRunStartTime = DateTime.Now;
			_beforeScenarioTimeSpent = TimeSpan.Zero;
			_afterScenarioTimeSpent = TimeSpan.Zero;
			_clearDataTimeSpent = TimeSpan.Zero;
			_createDataTimeSpent = TimeSpan.Zero;
			_scenarioCount = 0;

			Browser.PrepareForTestRun();
			if (!Browser.IsStarted())
				Browser.Start();

			try
			{
				TestControllerMethods.BeforeTestRun();

				TestSiteConfigurationSetup.Setup();

				log4net.Config.XmlConfigurator.Configure();
				EventualTimeouts.Set(TimeSpan.FromSeconds(5));

				TestDataSetup.CreateDataSource();

				TestDataSetup.SetupFakeState();
				TestDataSetup.CreateMinimumTestData();
				CreateData();

				if (ExperimentalDataMode.ForEachScenario)
					TestDataSetup.BackupCcc7Data();
			}
			catch(Exception)
			{
				Browser.Close();
			}

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

			if (ExperimentalDataMode.ForEachScenario)
			{
				TestDataSetup.RestoreCcc7Data();
				MakeSureUowConnectionWorks();
			}

			TestDataSetup.ClearAnalyticsData();

			var spentTime = DateTime.Now.Subtract(startTime);
			_beforeScenarioTimeSpent = _beforeScenarioTimeSpent.Add(spentTime);

			Log.Write("BeforeScenario took " + spentTime);
		}

		private static void MakeSureUowConnectionWorks()
		{
			var tries = 10;
			var sleep = TimeSpan.FromSeconds(1);
			while (tries > 0)
			{
				try
				{
					TestDataSetup.UnitOfWorkAction(uow =>
					                               	{
					                               		var repository = new PersonRepository(uow);
					                               		var person = repository.Get(TestData.PersonThatCreatesTestData.Id.Value);
					                               		person.Email = Guid.NewGuid().ToString();
					                               		uow.PersistAll();
					                               	});
				}
				catch (DataSourceException exception)
				{
					if (exception.Message != "Cannot start transaction")
						throw;
					if (tries == 0)
						throw;
					Thread.Sleep(sleep);
					tries--;
					continue;
				}
				return;
			}
		}

		[AfterScenario]
		public void AfterScenario()
		{
			var startTime = DateTime.Now;

			HandleScenarioException();
			//if (Browser.IsStarted())
			//    Browser.Close();
			TestControllerMethods.AfterScenario();

			var spentTime = DateTime.Now.Subtract(startTime);
			_afterScenarioTimeSpent = _afterScenarioTimeSpent.Add(spentTime);

			Log.Write("AfterScenario took " + spentTime);
			Log.Write("Run as taken " + DateTime.Now.Subtract(_testRunStartTime));

			Log.Write("Scenario time " + DateTime.Now.Subtract(_testRunScenariosStartTime));
			Log.Write("BeforeScenario time " + _beforeScenarioTimeSpent);
			Log.Write("AfterScenario time " + _afterScenarioTimeSpent);
			Log.Write("ClearData time " + _clearDataTimeSpent);
			Log.Write("CreateData time " + _createDataTimeSpent);

			Log.Write("BeforeScenario time/scenario " + TimeSpan.FromMilliseconds(_beforeScenarioTimeSpent.TotalMilliseconds / _scenarioCount));
			Log.Write("AfterScenario time/scenario " + TimeSpan.FromMilliseconds(_afterScenarioTimeSpent.TotalMilliseconds / _scenarioCount));
			Log.Write("ClearData time/scenario " + TimeSpan.FromMilliseconds(_clearDataTimeSpent.TotalMilliseconds / _scenarioCount));
			Log.Write("CreateData time/scenario " + TimeSpan.FromMilliseconds(_createDataTimeSpent.TotalMilliseconds / _scenarioCount));
		}

		[AfterTestRun]
		public static void AfterTestRun()
		{
			var startTime = DateTime.Now;

			if (Browser.IsStarted())
				Browser.Close();

			TestSiteConfigurationSetup.TearDown();

			Log.Write("AfterTestRun took " + DateTime.Now.Subtract(startTime));
		}




		//private static void PrepareData()
		//{
		//    //ITeleoptiPrincipal principal = null;
		//    //var clearCcc7Data = Task.Factory.StartNew(TestDataSetup.ClearCcc7Data);
		//    //var clearAnalyticsData = Task.Factory.StartNew(TestDataSetup.ClearAnalyticsData);
		//    //var setupFakeState = Task.Factory.StartNew(() =>
		//    //                                            {
		//    //                                                TestDataSetup.SetupFakeState();
		//    //                                                principal = Thread.CurrentPrincipal as ITeleoptiPrincipal;
		//    //                                            });
		//    //var createTestData = Task.Factory.StartNew(() =>
		//    //                                            {
		//    //                                                Task.WaitAll(clearCcc7Data, setupFakeState);
		//    //                                                Thread.CurrentPrincipal = principal;

		//    //                                                TestDataSetup.CreateLegacyTestData();

		//    //                                                DataContext.Data().Setup(new CommonContract());
		//    //                                                DataContext.Data().Setup(new ContractScheduleWith2DaysOff());
		//    //                                                DataContext.Data().Setup(new CommonScenario());
		//    //                                                DataContext.Data().Setup(new SecondScenario());
		//    //                                                DataContext.Data().Persist();

		//    //                                            });

		//    //Task.WaitAll(clearCcc7Data, clearAnalyticsData, setupFakeState, createTestData);
		//    //Thread.CurrentPrincipal = principal;
		//    //return;

		//    ClearCcc7Data();
		//    CreateData();
		//}

		private static void CreateData()
		{
			var startTime = DateTime.Now;

			TestDataSetup.CreateLegacyTestData();

			DataContext.Data().Setup(new CommonContract());
			DataContext.Data().Setup(new ContractScheduleWith2DaysOff());
			DataContext.Data().Setup(new CommonScenario());
			DataContext.Data().Setup(new SecondScenario());
			DataContext.Data().Persist();

			var spentTime = DateTime.Now.Subtract(startTime);
			_createDataTimeSpent = _createDataTimeSpent.Add(spentTime);
		}

		private static void ClearCcc7Data()
		{
			var startTime = DateTime.Now;

			TestDataSetup.ClearCcc7Data();

			var spentTime = DateTime.Now.Subtract(startTime);
			_clearDataTimeSpent = _clearDataTimeSpent.Add(spentTime);
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