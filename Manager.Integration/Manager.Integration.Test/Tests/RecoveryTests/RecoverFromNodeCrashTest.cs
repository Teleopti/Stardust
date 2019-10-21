using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manager.Integration.Test.Database;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Initializers;
using Manager.Integration.Test.Models;
using Manager.Integration.Test.Params;
using Manager.Integration.Test.Timers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Manager.Integration.Test.Tests.RecoveryTests
{
	[TestFixture]
	public class RecoverFromNodeCrashTest : InitializeAndFinalizeOneManagerAndNodes
	{
		[Test]
		public void ShouldConsiderNodeAsDeadWhenInactiveAndSetJobResultToFatal()
		{
			var startedTest = DateTime.UtcNow;
			
			var waitForJobToStartEvent = new ManualResetEventSlim();
			var waitForNodeToEndEvent = new ManualResetEventSlim();
			
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
				if (nodes.Any() && nodes.All(node => node.Alive == false))
				{
					waitForNodeToEndEvent.Set();
				}
			};
			checkTablesInManagerDbTimer.JobTimer.Start();
			checkTablesInManagerDbTimer.WorkerNodeTimer.Start();
			
            // ###############################
            var failingJobParams = new FailingJobParams("Error message - THIS SHOULD CRASH");
            var failingJobParamsJson = JsonConvert.SerializeObject(failingJobParams);
            var crashingJOb = new  JobQueueItem
            {
                Name = "Crash Job",
                Serialized = failingJobParamsJson,
                Type = "NodeTest.JobHandlers.FailingJobParams",
                CreatedBy = "Test"
            };

            // #################################

			var jobQueueItem =
				JobHelper.GenerateTestJobRequests(1, 5*60).First();
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

			//Give manager a couple of seconds to requeue job
			Thread.Sleep(TimeSpan.FromSeconds(2));

			var jobs = checkTablesInManagerDbTimer.ManagerDbRepository.Jobs;

			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.WorkerNodes.All(node => node.Alive == false), "Worker Node should not be alive.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any(),"Job should be in queue.");
			Assert.IsTrue(!jobs.Any(), "Job should be empty.");

			checkTablesInManagerDbTimer.Dispose();

			var endedTest = DateTime.UtcNow;

			var description =
                $"Creates Node Failure jobs with {NumberOfManagers} manager and {NumberOfNodes} nodes.";

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
											  description,
											  startedTest,
											  endedTest);
		}
	}
}