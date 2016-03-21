using System.Collections.Generic;
using Manager.Integration.Test.Constants;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Initializers;
using Manager.Integration.Test.Tasks;
using Manager.Integration.Test.Timers;
using NUnit.Framework;

namespace Manager.Integration.Test.LoadTests
{
	[TestFixture]
	public class OneManagerAndOneNodeLoadTests : InitialzeAndFinalizeOneManagerAndOneNodeNoLoadBalancer
	{
		[Test]
		public void YourTestGoesHere()
		{
			var createNewJobRequests = JobHelper.GenerateTestJobParamsRequests(1);

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

			CancellationTokenSource.Cancel();
			taskHlp.Dispose();

			foreach (var jobManagerTaskCreator in jobManagerTaskCreators)
			{
				jobManagerTaskCreator.Dispose();
			}
		}
	}
}