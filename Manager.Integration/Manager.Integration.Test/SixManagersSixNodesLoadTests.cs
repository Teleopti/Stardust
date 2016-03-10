﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Manager.Integration.Test.Constants;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Notifications;
using Manager.Integration.Test.Tasks;
using Manager.Integration.Test.Timers;
using Manager.Integration.Test.Validators;
using NUnit.Framework;

namespace Manager.Integration.Test
{
	[TestFixture]
	class SixManagersSixNodesLoadTests
	{
			private string ManagerDbConnectionString { get; set; }
			private Task Task { get; set; }
			private AppDomainTask AppDomainTask { get; set; }
			private CancellationTokenSource CancellationTokenSource { get; set; }

			private  bool _clearDatabase = true;
			private  string _buildMode = "Debug";

			private static readonly ILog Logger =
				LogManager.GetLogger(typeof(OneManagerAndFiveNodesLoadTests));

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

				Task = AppDomainTask.StartTask(numberOfManagers: 6,
											   numberOfNodes: 6,
											   cancellationTokenSource: CancellationTokenSource);
				Thread.Sleep(TimeSpan.FromSeconds(2));
				logMessage("Finished TestFixtureSetUp");
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

			private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
			{
				var exp = e.ExceptionObject as Exception;
				if (exp != null)
				{
					LogHelper.LogFatalWithLineNumber(exp.Message,
													 Logger,
													 exp);
				}
			}

			[Test, Ignore]
			public void ShouldBeAbleToExecuteManyFastSuccessJobRequestTest()
			{
				logMessage("Start.");

				var createNewJobRequests = JobHelper.GenerateFastJobParamsRequests(50);
				var checkJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(createNewJobRequests.Count,
																				StatusConstants.SuccessStatus,
																				StatusConstants.DeletedStatus,
																				StatusConstants.FailedStatus,
																				StatusConstants.CanceledStatus);

				//---------------------------------------------
				// Create timeout time.
				//---------------------------------------------
				var timeout =
					JobHelper.GenerateTimeoutTimeInMinutes(createNewJobRequests.Count);

				//---------------------------------------------
				// Create jobs.
				//---------------------------------------------
				var jobManagerTaskCreators = new List<JobManagerTaskCreator>();
				foreach (var jobRequestModel in createNewJobRequests)
				{
					var jobManagerTaskCreator = new JobManagerTaskCreator(checkJobHistoryStatusTimer);
					jobManagerTaskCreator.CreateNewJobToManagerTask(jobRequestModel);
					jobManagerTaskCreators.Add(jobManagerTaskCreator);
				}

				//---------------------------------------------
				// Execute all jobs. 
				//---------------------------------------------
				var startJobTaskHelper = new StartJobTaskHelper();
				var taskHelper = startJobTaskHelper.ExecuteCreateNewJobTasks(jobManagerTaskCreators,
																			 CancellationTokenSource,
																			 TimeSpan.FromMilliseconds(50));

				//---------------------------------------------
				// Wait for all jobs to finish.
				//---------------------------------------------
				checkJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

				Assert.IsTrue(checkJobHistoryStatusTimer.Guids.Count == createNewJobRequests.Count);
				Assert.IsTrue(checkJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.SuccessStatus));

				//---------------------------------------------
				// Cancel tasks.
				//---------------------------------------------
				CancellationTokenSource.Cancel();

				//---------------------------------------------
				// Dispose.
				//---------------------------------------------
				foreach (var jobManagerTaskCreator in jobManagerTaskCreators)
				{
					jobManagerTaskCreator.Dispose();
				}

				taskHelper.Dispose();
			
				logMessage("Finished.");
			}

		[Test]
		public void ShouldBeAbleToCreateManySuccessJobRequestTest()
		{
			LogHelper.LogDebugWithLineNumber("Start.",
											 Logger);

			var createNewJobRequests = JobHelper.GenerateTestJobParamsRequests(50);
			var checkJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(createNewJobRequests.Count,
																			StatusConstants.SuccessStatus,
																			StatusConstants.DeletedStatus,
																			StatusConstants.FailedStatus,
																			StatusConstants.CanceledStatus);

			//---------------------------------------------
			// Create timeout time.
			//---------------------------------------------
			var timeout =
				JobHelper.GenerateTimeoutTimeInMinutes(createNewJobRequests.Count);

			//---------------------------------------------
			// Create jobs.
			//---------------------------------------------
			var jobManagerTaskCreators = new List<JobManagerTaskCreator>();
			foreach (var jobRequestModel in createNewJobRequests)
			{
				var jobManagerTaskCreator =
					new JobManagerTaskCreator(checkJobHistoryStatusTimer);
				jobManagerTaskCreator.CreateNewJobToManagerTask(jobRequestModel);
				jobManagerTaskCreators.Add(jobManagerTaskCreator);
			}

			logMessage("Waiting for all nodes to start up.");

			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();
			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(6,
																  sqlNotiferCancellationTokenSource,
																  IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
			task.Start();

			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(30));
			sqlNotifier.Dispose();

			logMessage("All nodes has started.");

			//---------------------------------------------
			// Execute all jobs. 
			//---------------------------------------------
			var startJobTaskHelper = new StartJobTaskHelper();

			var taskHelper = startJobTaskHelper.ExecuteCreateNewJobTasks(jobManagerTaskCreators,
																		 CancellationTokenSource,
																		 TimeSpan.FromMilliseconds(50));

			//---------------------------------------------
			// Wait for all jobs to finish.
			//---------------------------------------------
			checkJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

			Assert.IsTrue(checkJobHistoryStatusTimer.Guids.Count == createNewJobRequests.Count);
			Assert.IsTrue(checkJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.SuccessStatus));

			//---------------------------------------------
			// Cancel tasks.
			//---------------------------------------------
			CancellationTokenSource.Cancel();

			//---------------------------------------------
			// Dispose.
			//---------------------------------------------
			foreach (var jobManagerTaskCreator in jobManagerTaskCreators)
			{
				jobManagerTaskCreator.Dispose();
			}

			taskHelper.Dispose();
			logMessage("Finished.");
		}
	}
}
