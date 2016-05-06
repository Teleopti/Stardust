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
	internal class DetectNodeFailureTest : InitialzeAndFinalizeOneManagerAndOneNodeWait
	{
		[Test]
		public void ShouldConsiderNodeAsDeadWhenInactiveAndSetJobResultToFatal()
		{
			var startedTest = DateTime.UtcNow;
			
			var waitForJobToStartEvent = new ManualResetEventSlim();
			var waitForNodeToEndEvent = new ManualResetEventSlim();

			//---------------------------------------------------------
			// Database validator.
			//---------------------------------------------------------
			var checkTablesInManagerDbTimer =
				new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 100);

			checkTablesInManagerDbTimer.ReceivedJobItem += (sender, items) =>
			{
				if (items.Any() && items.All(job => job.Started != null))
				{
					waitForJobToStartEvent.Set();
				}
			};
			checkTablesInManagerDbTimer.ReceivedWorkerNodesData += (sender, nodes) =>
			{
				if (nodes.Any() && nodes.All(node => node.Alive == false))
				{
					waitForNodeToEndEvent.Set();
				}
			};
			checkTablesInManagerDbTimer.JobTimer.Start();
			checkTablesInManagerDbTimer.WorkerNodeTimer.Start();
			

			var jobQueueItem =
				JobHelper.GenerateTestJobTimerRequests(1, TimeSpan.FromMinutes(5)).First();
			var jobId = HttpRequestManager.AddJob(jobQueueItem);
			
			waitForJobToStartEvent.Wait(TimeSpan.FromMinutes(2));

			Task<string> taskShutDownNode = new Task<string>(() =>
			{
				string res = IntegrationControllerApiHelper.ShutDownNode(HttpSender,
																		"Node1.config").Result;
				return res;
			});

			taskShutDownNode.Start();
			taskShutDownNode.Wait();
			
			waitForNodeToEndEvent.Wait(TimeSpan.FromMinutes(1));

			var jobs = checkTablesInManagerDbTimer.ManagerDbRepository.Jobs;

			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.WorkerNodes.All(node => node.Alive == false), "Worker Node should not be alive.");
			Assert.IsTrue(!checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any(),"Job queue should be empty.");
			Assert.IsTrue(jobs.Any(), "Job should not be empty.");
			Assert.IsTrue(jobs.Any(job => job.Result.StartsWith("Fatal Node Failure",StringComparison.InvariantCultureIgnoreCase)),
						"All jobs shall have status of Fatal Node Failure.");

			checkTablesInManagerDbTimer.Dispose();

			var endedTest = DateTime.UtcNow;

			var description =
				string.Format("Creates Node Failure jobs with {0} manager and {1} nodes.",
							  NumberOfManagers,
							  NumberOfNodes);

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
											  description,
											  startedTest,
											  endedTest);
		}
		
	}
}