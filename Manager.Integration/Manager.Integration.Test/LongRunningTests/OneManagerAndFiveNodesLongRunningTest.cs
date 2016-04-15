using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net.Config;
using Manager.Integration.Test.Constants;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Notifications;
using Manager.Integration.Test.Tasks;
using Manager.Integration.Test.Timers;
using Manager.Integration.Test.Validators;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;
using NUnit.Framework;

namespace Manager.Integration.Test.LongRunningTests
{
	[TestFixture]
	public class OneManagerAndFiveNodesLongRunningTest
	{
		private string ManagerDbConnectionString { get; set; }
		private Task Task { get; set; }
		private AppDomainTask AppDomainTask { get; set; }
		private CancellationTokenSource CancellationTokenSource { get; set; }

#if (DEBUG)
		private const bool ClearDatabase = true;
		private const string BuildMode = "Debug";

#else
		private const bool ClearDatabase = true;
		private const string BuildMode = "Release";
#endif

		private void LogMessage(string message)
		{
			this.Log().DebugWithLineNumber(message);
		}

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			ManagerDbConnectionString =
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;

			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

			LogMessage("Start TestFixtureSetUp");

			if (ClearDatabase)
			{
				DatabaseHelper.TryClearDatabase(ManagerDbConnectionString);
			}
			CancellationTokenSource = new CancellationTokenSource();

			AppDomainTask = new AppDomainTask(BuildMode);

			Task = AppDomainTask.StartTask(numberOfManagers: 1,
			                               numberOfNodes: 10,
			                               useLoadBalancerIfJustOneManager: true,
			                               cancellationTokenSource: CancellationTokenSource);
			Thread.Sleep(TimeSpan.FromSeconds(2));
			LogMessage("Finished TestFixtureSetUp");
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			LogMessage("Start TestFixtureTearDown");
			if (AppDomainTask != null)
			{
				AppDomainTask.Dispose();
			}
			LogMessage("Finished TestFixtureTearDown");
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var exp = e.ExceptionObject as Exception;

			if (exp != null)
			{
				this.Log().FatalWithLineNumber(exp.Message,
				                               exp);
			}
		}

		/// <summary>
		///     DO NOT FORGET TO RUN COMMAND BELOW AS ADMINISTRATOR.
		///     netsh http add urlacl url=http://+:9050/ user=everyone listen=yes
		/// </summary>
		[Test]
		public void ShouldBeAbleToCreateManySuccessJobRequestTest()
		{
			this.Log().DebugWithLineNumber("Start.");

			var startedTest = DateTime.UtcNow;

			LogMessage("Waiting for all nodes to start up.");

			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();
			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(10,
			                                                      sqlNotiferCancellationTokenSource,
			                                                      IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
			task.Start();

			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(30));
			sqlNotifier.Dispose();

			LogMessage("All nodes has started.");


			Task task1 = new Task(() =>
			{
				int loop = 1;

				while (loop <= 5000)
				{
					loop++;

					//---------------------------------------------
					// Create jobs.
					//---------------------------------------------
					var createNewJobRequests = JobHelper.GenerateFastJobParamsRequests(10);

					var checkJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(createNewJobRequests.Count,
																					StatusConstants.SuccessStatus,
																					StatusConstants.DeletedStatus,
																					StatusConstants.FailedStatus,
																					StatusConstants.CanceledStatus);


					var jobManagerTaskCreators = new List<JobManagerTaskCreator>();

					foreach (var jobRequestModel in createNewJobRequests)
					{
						var jobManagerTaskCreator =
							new JobManagerTaskCreator(checkJobHistoryStatusTimer);

						jobManagerTaskCreator.CreateNewJobToManagerTask(jobRequestModel);

						jobManagerTaskCreators.Add(jobManagerTaskCreator);
					}

					//---------------------------------------------
					// Execute all jobs. 
					//---------------------------------------------
					var startJobTaskHelper = new StartJobTaskHelper();

					var taskHelper = startJobTaskHelper.ExecuteCreateNewJobTasks(jobManagerTaskCreators,
																				 CancellationTokenSource,
																				 TimeSpan.FromMilliseconds(100));

					var timeout =
						JobHelper.GenerateTimeoutTimeInMinutes(createNewJobRequests.Count);

					checkJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);
				}

			});

			Task task2 = new Task(() =>
			{
				int loop = 1;

				while (loop <= 5000)
				{
					loop++;

					//---------------------------------------------
					// Create jobs.
					//---------------------------------------------
					var createNewJobRequests = JobHelper.GenerateFastJobParamsRequests(5);

					var checkJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(createNewJobRequests.Count,
																					StatusConstants.SuccessStatus,
																					StatusConstants.DeletedStatus,
																					StatusConstants.FailedStatus,
																					StatusConstants.CanceledStatus);


					var jobManagerTaskCreators = new List<JobManagerTaskCreator>();

					foreach (var jobRequestModel in createNewJobRequests)
					{
						var jobManagerTaskCreator =
							new JobManagerTaskCreator(checkJobHistoryStatusTimer);

						jobManagerTaskCreator.CreateNewJobToManagerTask(jobRequestModel);

						jobManagerTaskCreators.Add(jobManagerTaskCreator);
					}

					//---------------------------------------------
					// Execute all jobs. 
					//---------------------------------------------
					var startJobTaskHelper = new StartJobTaskHelper();

					var taskHelper = startJobTaskHelper.ExecuteCreateNewJobTasks(jobManagerTaskCreators,
																				 CancellationTokenSource,
																				 TimeSpan.FromMilliseconds(100));

					var timeout =
						JobHelper.GenerateTimeoutTimeInMinutes(createNewJobRequests.Count);

					checkJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);
				}

			});

			task1.Start();
			task2.Start();

			Task.WaitAll(task1, task2);


			var endedTest = DateTime.UtcNow;

			string description =
				string.Format("Creates {0} FAST jobs with {1} manager and {2} nodes.",
								50000,
								1,
								10);

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
											  description,
											  startedTest,
											  endedTest);

		}
	}
}