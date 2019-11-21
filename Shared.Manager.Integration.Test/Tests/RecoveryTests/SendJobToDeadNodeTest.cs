using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manager.Integration.Test.Database;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Initializers;
using Manager.Integration.Test.Timers;
using NUnit.Framework;

namespace Manager.Integration.Test.Tests.RecoveryTests
{
	[TestFixture]
	class SendJobToDeadNodeTest : InitializeAndFinalizeOneManagerAndNodes
	{
		public SendJobToDeadNodeTest():base(2)
		{
		}
		
		[Test]
		public void ShouldHandleMultipleJobsUsingAllNodesAvailable()
		{
			var numberOfNodes = NumberOfNodes;
			var startedTest = DateTime.UtcNow;
			var numberOfJobs = 15;
			var waitForJobToFinishEvent = new ManualResetEventSlim();
			var waitForNodesToStartEvent = new ManualResetEventSlim();
			
			var checkTablesInManagerDbTimer =
				new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 100);
			checkTablesInManagerDbTimer.GetJobItems += (sender, items) =>
			{
				if (items.Count(j => j.Ended != null) == numberOfJobs)
				{
					waitForJobToFinishEvent.Set();
				}
			};
			checkTablesInManagerDbTimer.GetWorkerNodes += (sender, nodes) =>
			{
				if (nodes.Count == numberOfNodes)
				{
					waitForNodesToStartEvent.Set();
				}
			};

			checkTablesInManagerDbTimer.JobTimer.Start();
			checkTablesInManagerDbTimer.WorkerNodeTimer.Start();
			waitForNodesToStartEvent.Wait();

			var jobQueueItems = JobHelper.GenerateTestJobRequests(numberOfJobs, 1);	
			jobQueueItems.ForEach(jobQueueItem => HttpRequestManager.AddJob(jobQueueItem));
			
			var jobsFinishedWithoutTimeout = waitForJobToFinishEvent.Wait(TimeSpan.FromSeconds(120));
			
			Assert.IsTrue(jobsFinishedWithoutTimeout, "Timeout on Finishing jobs");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.WorkerNodes.Count == numberOfNodes, "There should be two nodes registered");
			Assert.IsFalse(checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any(), "Job queue should be empty.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any(), "Job should not be empty.");
			Assert.AreEqual(checkTablesInManagerDbTimer.ManagerDbRepository.WorkerNodes.Count,
				checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Select(j => j.SentToWorkerNodeUri).Distinct().Count());
		
			checkTablesInManagerDbTimer.Dispose();
			var endedTest = DateTime.UtcNow;
			var description =
                $"Creates {numberOfJobs} Test Timer jobs with {NumberOfManagers} manager and {NumberOfNodes + 1} nodes.";

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
											  description,
											  startedTest,
											  endedTest);
		}
		
		[Test, Ignore("A little flaky still")]
		public void ShouldHandleNodeDisappearingTemporarily()
		{
			var startedTest = DateTime.UtcNow;
			var numberOfJobs = 20;
			var waitForJobToFinishEvent = new ManualResetEventSlim();
			var waitForNodeToStartEvent = new ManualResetEventSlim();
			var waitForAllNodesToFinishAJob = new ManualResetEventSlim();
			
			var checkTablesInManagerDbTimer =
				new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 300);
			checkTablesInManagerDbTimer.GetJobItems += (sender, items) =>
			{
				if (items.Count(job => job.Ended != null) == numberOfJobs)
				{
					waitForJobToFinishEvent.Set();
				}

				if (items.Where(job => job.Ended != null).GroupBy(job => job.SentToWorkerNodeUri).Count() == 2)
				{
					waitForAllNodesToFinishAJob.Set();
				}
			};
			checkTablesInManagerDbTimer.GetWorkerNodes += (sender, nodes) =>
			{
				if (nodes.Count == 2)
				{
					waitForNodeToStartEvent.Set();
				}
			};

			checkTablesInManagerDbTimer.JobTimer.Start();
			checkTablesInManagerDbTimer.WorkerNodeTimer.Start();

			waitForNodeToStartEvent.Wait();
			var jobQueueItemsBatch1 = JobHelper.GenerateTestJobRequests(numberOfJobs, 1);
			jobQueueItemsBatch1.ForEach(jobQueueItem => HttpRequestManager.AddJob(jobQueueItem));

			var allNodesRunningWithoutTimeout = waitForAllNodesToFinishAJob.Wait(TimeSpan.FromSeconds(60));
			Assert.IsTrue(allNodesRunningWithoutTimeout, "Timeout when waiting for all nodes to finish at least one job");
			Assert.AreNotEqual(0, checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueCount);
			
			Task<string> taskShutDownNode2 = new Task<string>(() =>
			{
				string res = IntegrationControllerApiHelper.ShutDownNode(HttpSender,"Node2.config").Result;
				return res;
			});
			taskShutDownNode2.RunSynchronously();
			
			//restart second node
			Task<string> taskStartNewNodeRestarted = new Task<string>(() =>
			{
				string res = IntegrationControllerApiHelper.StartNewNode(HttpSender).Result;
				return res;
			});
			taskStartNewNodeRestarted.RunSynchronously();
			
			var jobsFinishedWithoutTimeout = waitForJobToFinishEvent.Wait(TimeSpan.FromSeconds(200));
			Assert.IsTrue(jobsFinishedWithoutTimeout, "Timeout on Finishing jobs");
			
			//
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.WorkerNodes.Count == 3, "There should be two nodes registered");
			Assert.IsFalse(checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any(), "Job queue should be empty.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any(), "Job should not be empty.");
			Assert.AreEqual(checkTablesInManagerDbTimer.ManagerDbRepository.WorkerNodes.Count,
				checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Select(j => j.SentToWorkerNodeUri).Distinct().Count());
		
			checkTablesInManagerDbTimer.Dispose();

			var endedTest = DateTime.UtcNow;
			
			var description =
                $"Creates {numberOfJobs} Test Timer jobs with {NumberOfManagers} manager and {NumberOfNodes + 2} nodes.";

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
											  description,
											  startedTest,
											  endedTest);
		}
	}
}
