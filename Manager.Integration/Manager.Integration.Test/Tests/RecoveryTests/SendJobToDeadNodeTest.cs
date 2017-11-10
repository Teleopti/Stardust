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
	class SendJobToDeadNodeTest : InitialzeAndFinalizeOneManagerAndOneNode
	{

		[Test]
		public void ShouldSendAJobToTheNextNodeIfTheFirstIsNotResponding()
		{
			var startedTest = DateTime.UtcNow;

			var waitForJobToStartEvent = new ManualResetEventSlim();
			var waitForNodeToStartEvent = new ManualResetEventSlim();
			

			var checkTablesInManagerDbTimer =
				new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 100);
			checkTablesInManagerDbTimer.GetJobItems += (sender, items) =>
			{
				if (items.Any() && items.All(job => job.Started != null))
				{
					waitForJobToStartEvent.Set();
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
	
			//kill second node
			Task<string> taskShutDownNode = new Task<string>(() =>
			{
				string res = IntegrationControllerApiHelper.ShutDownNode(HttpSender,"Node2.config").Result;
				return res;
			});
			taskShutDownNode.Start();
			
			var jobQueueItem = JobHelper.GenerateTestJobRequests(1, 1).First();
			var jobId = HttpRequestManager.AddJob(jobQueueItem);

			//Should not take long time to assign job, if it takes more than some seconds 
			// otherwise it is maybe the background timer who assigned the job
			waitForJobToStartEvent.Wait(TimeSpan.FromSeconds(10));

			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.WorkerNodes.Count == 2, "There should be two nodes registered");
			Assert.IsTrue(!checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any(), "Job queue should be empty.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any(), "Job should not be empty.");

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
