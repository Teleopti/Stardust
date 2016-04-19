using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manager.Integration.Test.Constants;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Initializers;
using Manager.Integration.Test.Notifications;
using Manager.Integration.Test.Tasks;
using Manager.Integration.Test.Timers;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;
using NUnit.Framework;

namespace Manager.Integration.Test.FunctionalTests
{
	[TestFixture]
	public class OneManagerAndOneNodeTests : InitialzeAndFinalizeOneManagerAndOneNodeWait
	{
		private void LogMessage(string message)
		{
			this.Log().DebugWithLineNumber(message);
		}

		[Test]
		public void CancelWrongJobsTest()
		{
			LogMessage("Start.");

			var startedTest = DateTime.UtcNow;

			var createNewJobRequests =
				JobHelper.GenerateTestJobTimerRequests(1,TimeSpan.FromSeconds(30));

			LogMessage("( " + createNewJobRequests.Count + " ) jobs will be created.");

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
			var taskHlp = startJobTaskHelper.ExecuteCreateNewJobTasks(jobManagerTaskCreators,
			                                                          new CancellationTokenSource(),
			                                                          timeout);

			checkJobHistoryStatusTimer.GuidAddedEventHandler += (sender,
			                                                     args) =>
			{
				//-----------------------------------
				// Wait for job to start.
				//-----------------------------------
				var nodeStartedNotifier =
					new NodeStatusNotifier(ManagerDbConnectionString);
				nodeStartedNotifier.StartJobDefinitionStatusNotifier(args.Guid,
				                                                     "Started",
				                                                     new CancellationTokenSource());
				nodeStartedNotifier.JobDefinitionStatusNotify.Wait(timeout);

				//-----------------------------------
				// Send wrong id to cancel.
				//-----------------------------------
				var newGuid = Guid.NewGuid();

				var jobManagerTaskCreator = new JobManagerTaskCreator(checkJobHistoryStatusTimer);
				jobManagerTaskCreator.CreateDeleteJobToManagerTask(newGuid);
				jobManagerTaskCreator.StartAndWaitDeleteJobToManagerTask(timeout);
				jobManagerTaskCreator.Dispose();

				nodeStartedNotifier.Dispose();
			};

			checkJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

			Assert.IsTrue(checkJobHistoryStatusTimer.Guids.Count == createNewJobRequests.Count);
			Assert.IsTrue(checkJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.SuccessStatus));

			taskHlp.Dispose();

			foreach (var jobManagerTaskCreator in jobManagerTaskCreators)
			{
				jobManagerTaskCreator.Dispose();
			}

			var endedTest = DateTime.UtcNow;

			var description =
				string.Format("Creates {0} CANCEL WRONG jobs with {1} manager and {2} nodes.",
				              createNewJobRequests.Count,
				              NumberOfManagers,
				              NumberOfNodes);

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
			                                  description,
			                                  startedTest,
			                                  endedTest);

			LogMessage("Finished.");
		}

		[Test]
		public void CreateRequestShouldReturnCancelOrDeleteStatusesTest()
		{
			LogMessage("Start.");

			var startedTest = DateTime.UtcNow;

			var createNewJobRequests =
				JobHelper.GenerateTestJobTimerRequests(1, TimeSpan.FromMinutes(2));

			var timeout = JobHelper.GenerateTimeoutTimeInMinutes(createNewJobRequests.Count,
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

			Task.Factory.StartNew(() =>
			{
				CheckTablesInManagerDbTimer checkTablesInManagerDbTimer = new CheckTablesInManagerDbTimer(500);
				checkTablesInManagerDbTimer.JobTimer.Start();

				checkTablesInManagerDbTimer.ReceivedJobItem += (o, jobs) =>
				{
					if (jobs.Any())
					{
						var job = jobs.First();

						if (job.Started != null)
						{
							checkTablesInManagerDbTimer.JobTimer.Stop();

							var jobManagerTaskCreator =
								new JobManagerTaskCreator(checkJobHistoryStatusTimer);

							jobManagerTaskCreator.CreateDeleteJobToManagerTask(jobs.First().JobId);
							jobManagerTaskCreator.StartAndWaitDeleteJobToManagerTask(timeout);

							jobManagerTaskCreator.Dispose();

							checkTablesInManagerDbTimer.JobTimer.Dispose();
						}
					}
				};
			},
			CancellationTokenSource.Token);

			var startJobTaskHelper = new StartJobTaskHelper();

			var taskHlp = startJobTaskHelper.ExecuteCreateNewJobTasks(jobManagerTaskCreators,
			                                                          CancellationTokenSource,
			                                                          timeout);

			checkJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

			var condition =
				checkJobHistoryStatusTimer.Guids.Count == createNewJobRequests.Count;

			Assert.IsTrue(condition,
			              "Must have equal number of rows.");

			var allCancelOrDeleteStatus =
				checkJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.CanceledStatus ||
															 pair.Value.Contains(StatusConstants.CancelingStatus) ||
				                                             pair.Value == StatusConstants.DeletedStatus);

			Assert.IsTrue(allCancelOrDeleteStatus,
			              "All rows shall have CANCEL, CANCELING or DELETE status.");

			taskHlp.Dispose();

			foreach (var jobManagerTaskCreator in jobManagerTaskCreators)
			{
				jobManagerTaskCreator.Dispose();
			}


			var endedTest = DateTime.UtcNow;

			var description =
				string.Format("Creates {0} CANCEL jobs with {1} manager and {2} nodes.",
				              createNewJobRequests.Count,
				              NumberOfManagers,
				              NumberOfNodes);

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
			                                  description,
			                                  startedTest,
			                                  endedTest);

			LogMessage("Finished.");
		}


		[Test]
		public void JobShouldHaveStatusFailedIfFailedTest()
		{
			var startedTest = DateTime.UtcNow;

			LogMessage("Start.");

			var createNewJobRequests = JobHelper.GenerateFailingJobParamsRequests(1);

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
			var taskHlp = startJobTaskHelper.ExecuteCreateNewJobTasks(jobManagerTaskCreators,
			                                                          CancellationTokenSource,
			                                                          timeout);

			checkJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

			Assert.IsTrue(checkJobHistoryStatusTimer.Guids.Count == createNewJobRequests.Count);
			Assert.IsTrue(checkJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.FailedStatus));

			taskHlp.Dispose();
			foreach (var jobManagerTaskCreator in jobManagerTaskCreators)
			{
				jobManagerTaskCreator.Dispose();
			}

			var endedTest = DateTime.UtcNow;

			var description =
				string.Format("Creates {0} FAILED jobs with {1} manager and {2} nodes.",
				              createNewJobRequests.Count,
				              NumberOfManagers,
				              NumberOfNodes);

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
			                                  description,
			                                  startedTest,
			                                  endedTest);

			LogMessage("Finished.");
		}

		/// <summary>
		///     DO NOT FORGET TO RUN COMMAND BELOW AS ADMINISTRATOR.
		///     netsh http add urlacl url=http://+:9050/ user=everyone listen=yes
		/// </summary>
		[Test]
		public void ShouldBeAbleToCreateASuccessJobRequestTest()
		{
			var startedTest = DateTime.UtcNow;

			LogMessage("Start.");

			var createNewJobRequests = JobHelper.GenerateTestJobParamsRequests(1);

			LogMessage("( " + createNewJobRequests.Count + " ) jobs will be created.");

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

			var taskHlp = startJobTaskHelper.ExecuteCreateNewJobTasks(jobManagerTaskCreators,
			                                                          CancellationTokenSource,
			                                                          timeout);

			checkJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);


			Assert.IsTrue(checkJobHistoryStatusTimer.Guids.Count == createNewJobRequests.Count,
			              "Number of requests must be equal.");

			Assert.IsTrue(checkJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.SuccessStatus));

			CancellationTokenSource.Cancel();
			taskHlp.Dispose();

			foreach (var jobManagerTaskCreator in jobManagerTaskCreators)
			{
				jobManagerTaskCreator.Dispose();
			}

			LogMessage("Finished.");

			var endedTest = DateTime.UtcNow;

			var description =
				string.Format("Creates {0} TEST jobs with {1} manager and {2} nodes.",
				              createNewJobRequests.Count,
				              NumberOfManagers,
				              NumberOfNodes);

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
			                                  description,
			                                  startedTest,
			                                  endedTest);
		}
	}
}