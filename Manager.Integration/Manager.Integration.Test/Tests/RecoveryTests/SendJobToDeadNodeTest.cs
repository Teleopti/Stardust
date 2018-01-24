using System;
using System.Diagnostics;
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
	class SendJobToDeadNodeTest : InitialzeAndFinalizeOneManagerAndOneNode
	{
		[Test]
		public void ShouldHandleMultipleJobsUsingAllNodesAvailable()
		{
			var startedTest = DateTime.UtcNow;
			var numberOfJobs = 15;
			var waitForJobToFinishEvent = new ManualResetEventSlim();
			var waitForNodeToStartEvent = new ManualResetEventSlim();
			
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
				if (nodes.Count == 2)
				{
					waitForNodeToStartEvent.Set();
				}
			};

			checkTablesInManagerDbTimer.JobTimer.Start();
			checkTablesInManagerDbTimer.WorkerNodeTimer.Start();

			//start second node
			Task<string> taskStartNewNode = new Task<string>(() =>
			{
				string res = IntegrationControllerApiHelper.StartNewNode(HttpSender).Result;
				return res;
			});

			taskStartNewNode.Start();
			waitForNodeToStartEvent.Wait();

			var jobQueueItems = JobHelper.GenerateTestJobRequests(numberOfJobs, 1);	
			jobQueueItems.ForEach(jobQueueItem => HttpRequestManager.AddJob(jobQueueItem));
			
			var jobsFinishedWithoutTimeout = waitForJobToFinishEvent.Wait(TimeSpan.FromSeconds(60));
			
			Assert.IsTrue(jobsFinishedWithoutTimeout, "Timeout on Finishing jobs");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.WorkerNodes.Count == 2, "There should be two nodes registered");
			Assert.IsFalse(checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any(), "Job queue should be empty.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any(), "Job should not be empty.");
			Assert.AreEqual(checkTablesInManagerDbTimer.ManagerDbRepository.WorkerNodes.Count,
				checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Select(j => j.SentToWorkerNodeUri).Distinct().Count());
		
			checkTablesInManagerDbTimer.Dispose();
			var endedTest = DateTime.UtcNow;
			var description =
				string.Format("Creates {0} Test Timer jobs with {1} manager and {2} nodes.",
							  numberOfJobs,
							  NumberOfManagers,
							  NumberOfNodes+1);

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
											  description,
											  startedTest,
											  endedTest);
		}
		
		[Test, Ignore("WIP")]
		public void ShouldHandleMultipleJobsUsingAllNodesAvailable2()
		{
			var startedTest = DateTime.UtcNow;
			var numberOfJobs = 15;
			var waitForJobToFinishEvent = new ManualResetEventSlim();
			var waitForNodeToStartEvent = new ManualResetEventSlim();
			var waitForAllNodesToFinishAJob = new ManualResetEventSlim();
			//var waitForQueueEmptyEvent = new ManualResetEventSlim();

			var checkTablesInManagerDbTimer =
				new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 300);
			checkTablesInManagerDbTimer.GetJobItems += (sender, items) =>
			{
				if (items.Count(job => job.Ended != null) == numberOfJobs)
				{
					waitForJobToFinishEvent.Set();
				}

				if (items.Select(job => job.SentToWorkerNodeUri).Distinct().Count() == 2)
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

//			checkTablesInManagerDbTimer.GetJobQueueItems += (sender, jobItems) =>
//			{
//				if (jobItems.Count == 0)
//				{
//					waitForQueueEmptyEvent.Set();
//				}
//				else
//				{
//					Debug.WriteLine("Queue not empty: {0}", jobItems.Count);
//				}
//			};
			
			checkTablesInManagerDbTimer.JobTimer.Start();
			checkTablesInManagerDbTimer.WorkerNodeTimer.Start();

			//start second node
			Task<string> taskStartNewNode = new Task<string>(() =>
			{
				string res = IntegrationControllerApiHelper.StartNewNode(HttpSender).Result;
				return res;
			});

			taskStartNewNode.Start();
			waitForNodeToStartEvent.Wait();
	
			//kill second node
//			Task<string> taskShutDownNode = new Task<string>(() =>
//			{
//				string res = IntegrationControllerApiHelper.ShutDownNode(HttpSender,"Node1.config").Result;
//				return res;
//			});
//			taskShutDownNode.Start(); // async shutdown
			
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
			var jobsFinishedWithoutTimeout = waitForJobToFinishEvent.Wait(TimeSpan.FromSeconds(100));
			Assert.IsTrue(jobsFinishedWithoutTimeout, "Timeout on Finishing jobs");
			
			//
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.WorkerNodes.Count == 2, "There should be two nodes registered");
			Assert.IsFalse(checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any(), "Job queue should be empty.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any(), "Job should not be empty.");
			Assert.AreEqual(checkTablesInManagerDbTimer.ManagerDbRepository.WorkerNodes.Count,
				checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Select(j => j.SentToWorkerNodeUri).Distinct().Count());
		
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
