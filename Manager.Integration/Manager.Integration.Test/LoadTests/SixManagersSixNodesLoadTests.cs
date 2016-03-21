using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net.Config;
using Manager.Integration.Test.Constants;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Initializers;
using Manager.Integration.Test.Notifications;
using Manager.Integration.Test.Tasks;
using Manager.Integration.Test.Timers;
using Manager.Integration.Test.Validators;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;
using NUnit.Framework;

namespace Manager.Integration.Test.LoadTests
{
	[TestFixture]
	internal class SixManagersSixNodesLoadTests : InitialzeAndFinalizeSixManagersAndSixNodes
	{
		private void LogMessage(string message)
		{
			this.Log().DebugWithLineNumber(message);
		}

		[Test]
		public void ShouldBeAbleToCreateManySuccessJobRequestTest()
		{
			this.Log().DebugWithLineNumber("Start.");

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

			LogMessage("Waiting for all nodes to start up.");

			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();
			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

			var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(6,
			                                                      sqlNotiferCancellationTokenSource,
			                                                      IntegerValidators.Value1IsLargerThenOrEqualToValue2Validator);
			task.Start();

			sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(30));
			sqlNotifier.Dispose();

			LogMessage("All nodes has started.");

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
			LogMessage("Finished.");
		}
	}
}