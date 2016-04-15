using System;
using System.Collections.Generic;
using Manager.Integration.Test.Constants;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Initializers;
using Manager.Integration.Test.Tasks;
using Manager.Integration.Test.Timers;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;
using NUnit.Framework;

namespace Manager.Integration.Test.FunctionalTests
{
	[TestFixture]
	public class OneManagerAndZeroNodesTests : InitialzeAndFinalizeOneManagerAndZeroNodes
	{
		[Test]
		public void JobsShouldJustBeQueuedIfNoNodesTest()
		{
			var startedTest = DateTime.UtcNow;

			this.Log().DebugWithLineNumber("Start.");

			var createNewJobRequests =
				JobHelper.GenerateFastJobParamsRequests(1);

			this.Log().DebugWithLineNumber("( " + createNewJobRequests.Count + " ) jobs will be created.");


			var timeout =
				JobHelper.GenerateTimeoutTimeInSeconds(createNewJobRequests.Count, 10);

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

			taskHlp.Dispose();

			foreach (var jobManagerTaskCreator in jobManagerTaskCreators)
			{
				jobManagerTaskCreator.Dispose();
			}

			var endedTest = DateTime.UtcNow;

			string description =
				string.Format("Creates {0} TEST jobs with {1} manager and {2} nodes.",
								createNewJobRequests.Count,
								base.NumberOfManagers,
								base.NumberOfNodes);

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
											  description,
											  startedTest,
											  endedTest);

			this.Log().DebugWithLineNumber("Finished.");
		}
	}
}