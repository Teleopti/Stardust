using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Manager.Integration.Test.Constants;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Tasks;
using Manager.Integration.Test.Timers;
using Manager.IntegrationTest.Console.Host.Diagnostics;
using NUnit.Framework;

namespace Manager.Integration.Test.RecoveryTests
{
	[TestFixture]
	public class OneManagerAndOneNodeRecoveryTests
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof (OneManagerAndOneNodeRecoveryTests));

		private bool _clearDatabase = true;
		private string _buildMode = "Debug";

		private string ManagerDbConnectionString { get; set; }
		private Task Task { get; set; }
		private AppDomainTask AppDomainTask { get; set; }
		private CancellationTokenSource CancellationTokenSource { get; set; }

		private void logMessage(string message)
		{
			LogHelper.LogDebugWithLineNumber(message, Logger);
		}

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			ManagerDbConnectionString =
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;

			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));
			logMessage("Start TestFixtureSetUp");

#if (DEBUG)
			// Do nothing.
#else
            _clearDatabase = true;
            _buildMode = "Release";
#endif

			if (_clearDatabase)
			{
				DatabaseHelper.TryClearDatabase(ManagerDbConnectionString);
			}
			CancellationTokenSource = new CancellationTokenSource();

			AppDomainTask = new AppDomainTask(_buildMode);
			Task = AppDomainTask.StartTask(numberOfManagers: 1,
			                               numberOfNodes: 1,
			                               cancellationTokenSource: CancellationTokenSource);

			Thread.Sleep(TimeSpan.FromSeconds(2));
			logMessage("Finished TestFixtureSetUp");
		}

		private void CurrentDomain_UnhandledException(object sender,
		                                              UnhandledExceptionEventArgs e)
		{
			var exp = e.ExceptionObject as Exception;
			if (exp != null)
			{
				LogHelper.LogFatalWithLineNumber(exp.Message,
				                                 Logger,
				                                 exp);
			}
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			logMessage("Start TestFixtureTearDown");

			if (AppDomainTask != null)
			{
				AppDomainTask.Dispose();
			}

			logMessage("Finished TestFixtureTearDown");
		}

		/// <summary>
		///     DO NOT FORGET TO RUN COMMAND BELOW AS ADMINISTRATOR.
		///     netsh http add urlacl url=http://+:9050/ user=everyone listen=yes
		/// </summary>
		[Test]
		public void ShouldBeAbleToCreateASuccessJobRequestTest()
		{
			logMessage("Start.");

			var createNewJobRequests = JobHelper.GenerateTestJobParamsRequests(1);

			logMessage("( " + createNewJobRequests.Count + " ) jobs will be created.");

			var timeout =
				JobHelper.GenerateTimeoutTimeInMinutes(createNewJobRequests.Count,
				                                       2);
			//--------------------------------------------
			// Start actual test.
			//--------------------------------------------
			var jobManagerTaskCreators = new List<JobManagerTaskCreator>();
			var checkJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(createNewJobRequests.Count,
			                                                                StatusConstants.SuccessStatus,
			                                                                StatusConstants.DeletedStatus,
			                                                                StatusConstants.FailedStatus,
			                                                                StatusConstants.CanceledStatus);
			foreach (var jobRequestModel in createNewJobRequests)
			{
				var jobManagerTaskCreator = new JobManagerTaskCreator(checkJobHistoryStatusTimer);
				jobManagerTaskCreator.CreateNewJobToManagerTask(jobRequestModel);
				jobManagerTaskCreators.Add(jobManagerTaskCreator);
			}

			var startJobTaskHelper = new StartJobTaskHelper();
			var managerIntegrationStopwatch = new ManagerIntegrationStopwatch();

			var taskHlp = startJobTaskHelper.ExecuteCreateNewJobTasks(jobManagerTaskCreators,
			                                                          CancellationTokenSource,
			                                                          timeout);

			checkJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);
			var elapsedTime =
				managerIntegrationStopwatch.GetTotalElapsedTimeInSeconds();

			logMessage("Job took : " + elapsedTime + " seconds.");

			Assert.IsTrue(checkJobHistoryStatusTimer.Guids.Count == createNewJobRequests.Count,
			              "Number of requests must be equal.");

			Assert.IsTrue(checkJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.SuccessStatus));

			CancellationTokenSource.Cancel();
			taskHlp.Dispose();

			foreach (var jobManagerTaskCreator in jobManagerTaskCreators)
			{
				jobManagerTaskCreator.Dispose();
			}
			logMessage("Finished.");
		}
	}
}