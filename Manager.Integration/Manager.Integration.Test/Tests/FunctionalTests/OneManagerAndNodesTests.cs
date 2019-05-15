using System;
using System.Linq;
using System.Threading;
using Manager.Integration.Test.Database;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Initializers;
using Manager.Integration.Test.Timers;
using NUnit.Framework;

namespace Manager.Integration.Test.Tests.FunctionalTests
{
	[TestFixture]
	public class OneManagerAndNodesTests : InitializeAndFinalizeOneManagerAndNodes
	{
		[Test]
		public void CancelWrongJobsTest()
		{
			WaitForNodeToFinishWorking();
			var startedTest = DateTime.UtcNow;
			var manualResetEventSlim = new ManualResetEventSlim();
			var checkTablesInManagerDbTimer =
				new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 100);

			bool sentCancel = false;
			checkTablesInManagerDbTimer.GetJobItems += (sender, items) =>
			{
				if (items.Any() &&
				    items.All(job => job.Ended == null) && sentCancel == false)
				{
					sentCancel = true;
					HttpRequestManager.CancelJob(Guid.NewGuid());
				}
				if (items.Any() &&
					items.All(job => job.Ended != null))
				{
					manualResetEventSlim.Set();
				}
			};
			checkTablesInManagerDbTimer.JobTimer.Start();


			var jobQueueItem =
				JobHelper.GenerateTestJobRequests(1, 5).First();
			var jobId = HttpRequestManager.AddJob(jobQueueItem);
			
			manualResetEventSlim.Wait(TimeSpan.FromSeconds(20));

			Assert.IsTrue(!checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any(), "Job queue must be empty.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any(), "Jobs must have been added.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.All(job => job.Result.StartsWith("Success", StringComparison.InvariantCultureIgnoreCase)));

			checkTablesInManagerDbTimer.Dispose();

			var endedTest = DateTime.UtcNow;

			var description =
				string.Format("Creates CANCEL WRONG jobs with {0} manager and {1} nodes.",
				              NumberOfManagers,
				              NumberOfNodes);

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
			                                  description,
			                                  startedTest,
			                                  endedTest);
		}

		[Test]
		public void CreateRequestShouldReturnCancelStatusWhenJobHasStartedAndBeenCanceled()
		{
			WaitForNodeToFinishWorking();
			var startedTest = DateTime.UtcNow;
			var manualResetEventSlim = new ManualResetEventSlim();
			var checkTablesInManagerDbTimer = new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 100);


			bool sentCancel = false;
			checkTablesInManagerDbTimer.GetJobItems += (sender, items) =>
			{
				if (items.Any() &&
					items.All(job => job.Ended == null) && sentCancel == false)
				{
					sentCancel = true;
					HttpRequestManager.CancelJob(items.First().JobId);
				}
				if (items.Any() &&
					items.All(job => job.Ended != null))
				{
					manualResetEventSlim.Set();
				}
			};
			checkTablesInManagerDbTimer.JobTimer.Start();
			

			var jobQueueItem =
				JobHelper.GenerateTestJobRequests(1, 60).First();
			var jobId = HttpRequestManager.AddJob(jobQueueItem);


			manualResetEventSlim.Wait(TimeSpan.FromSeconds(10));

			Assert.IsTrue(!checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any(), "Job queue must be empty.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any(), "Jobs must have been added.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any(job => job.Result.StartsWith("Canceled", StringComparison.InvariantCultureIgnoreCase)));

			checkTablesInManagerDbTimer.Dispose();

			var endedTest = DateTime.UtcNow;

			var description =
				string.Format("Creates Cancel Job with {0} manager and {1} nodes.",
				              NumberOfManagers,
				              NumberOfNodes);

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
			                                  description,
			                                  startedTest,
			                                  endedTest);
		}

		private void WaitForNodeToStartWorking()
		{
			var working = false;
			while (!working)
			{
				working = HttpRequestManager.IsNodeWorking();
				Thread.Sleep(TimeSpan.FromMilliseconds(200));
			}
		}

		private void WaitForNodeToFinishWorking()
		{
			var working = true;
			while (working)
			{
				working = HttpRequestManager.IsNodeWorking();
				Thread.Sleep(TimeSpan.FromMilliseconds(200));
			}
		}



		[Test]
		public void NodeShouldNotGetStuckWhenCancellingOrFailingJobs()
		{
			WaitForNodeToFinishWorking();
			var jobRepository = new ManagerDbRepository(ManagerDbConnectionString);

			var jobQueueItemCancel = JobHelper.GenerateTestJobRequests(1,60).First();
			var jobQueueItemFail = JobHelper.GenerateFailingJobParamsRequests(1).First();
			var jobQueueItemSuccess = JobHelper.GenerateTestJobRequests(1, 2).First();

			var cancelId = HttpRequestManager.AddJob(jobQueueItemCancel);
			WaitForNodeToStartWorking();
			HttpRequestManager.CancelJob(cancelId);
			WaitForNodeToFinishWorking();

			Thread.Sleep(TimeSpan.FromSeconds(2));

			HttpRequestManager.AddJob(jobQueueItemFail);  
			Thread.Sleep(TimeSpan.FromSeconds(2)); //might be a risc it ends before we see it is working.. 
			WaitForNodeToFinishWorking();

			Thread.Sleep(TimeSpan.FromSeconds(2));

			HttpRequestManager.AddJob(jobQueueItemSuccess);
			WaitForNodeToStartWorking();
			WaitForNodeToFinishWorking();

			Thread.Sleep(TimeSpan.FromSeconds(2));

			var queueItems = jobRepository.JobQueueItems;
			var jobs = jobRepository.Jobs;
			Assert.IsTrue(!queueItems.Any(), "Job queue must be empty.");
			Assert.IsTrue(jobs.Any(), "Jobs must have been added.");
			Assert.IsTrue(jobs.Any(job => job.Result.StartsWith("Canceled", StringComparison.InvariantCultureIgnoreCase)));
			Assert.IsTrue(jobs.Any(job => job.Result.StartsWith("fail", StringComparison.InvariantCultureIgnoreCase)));
			Assert.IsTrue(jobs.Any(job => job.Result.StartsWith("success", StringComparison.InvariantCultureIgnoreCase)));
		}


		[Test]
		public void JobShouldHaveStatusFailedIfFailedTest()
		{
			WaitForNodeToFinishWorking();
			var startedTest = DateTime.UtcNow;
			var manualResetEventSlim = new ManualResetEventSlim();
			var checkTablesInManagerDbTimer =
				new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 100);


			checkTablesInManagerDbTimer.GetJobItems += (sender, items) =>
			{
				if (items.Any() &&
				    items.All(job => job.Ended != null))
				{
					if (!manualResetEventSlim.IsSet)
					{
						manualResetEventSlim.Set();
					}
				}
			};
			checkTablesInManagerDbTimer.JobTimer.Start();


			var failingJobQueueItem =
				JobHelper.GenerateFailingJobParamsRequests(1).First();
			var jobId = HttpRequestManager.AddJob(failingJobQueueItem);


			manualResetEventSlim.Wait(TimeSpan.FromSeconds(10));

			Assert.IsTrue(!checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any(), "Job queue must be empty.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any(), "Jobs must have been added.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.
				              Jobs.Any(job => job.Result.StartsWith("fail", StringComparison.InvariantCultureIgnoreCase)));

			var endedTest = DateTime.UtcNow;

			var description =
				string.Format("Creates FAILED jobs with {0} manager and {1} nodes.",
				              NumberOfManagers,
				              NumberOfNodes);

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
			                                  description,
			                                  startedTest,
			                                  endedTest);


			checkTablesInManagerDbTimer.Dispose();
		}


		[Test]
		public void ShouldBeAbleToCreateASuccessJobRequestTest()
		{
			WaitForNodeToFinishWorking();
			var startedTest = DateTime.UtcNow;
			var manualResetEventSlim = new ManualResetEventSlim();
			var checkTablesInManagerDbTimer =
				new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 2000);


			checkTablesInManagerDbTimer.GetJobItems += (sender, items) =>
			{
				if (items.Any() &&
				    items.All(job => job.Started != null && job.Ended != null))
				{
					manualResetEventSlim.Set();

				}
			};
			checkTablesInManagerDbTimer.JobTimer.Start();


			var jobQueueItem =
				JobHelper.GenerateTestJobRequests(1, 5).First();
			var jobId = HttpRequestManager.AddJob(jobQueueItem);
			

			manualResetEventSlim.Wait(TimeSpan.FromSeconds(30));

			Assert.IsTrue(!checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any(), "Should not be any jobs left in queue.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any(), "There should be jobs in jobs table.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.
				              Jobs.All(job => job.Result.StartsWith("success", StringComparison.InvariantCultureIgnoreCase)));

			checkTablesInManagerDbTimer.Dispose();

			var endedTest = DateTime.UtcNow;

			var description =
				string.Format("Creates {0} Test Timer jobs with {1} manager and {2} nodes.",
				              1,
				              NumberOfManagers,
				              NumberOfNodes);

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
			                                  description,
			                                  startedTest,
			                                  endedTest);
		}
		
	}
}