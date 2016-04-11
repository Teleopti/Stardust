using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Manager.Integration.Test.Constants;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Initializers;
using Manager.Integration.Test.Tasks;
using Manager.Integration.Test.Timers;
using Manager.IntegrationTest.Console.Host.Diagnostics;
using Manager.IntegrationTest.Console.Host.Helpers;
using NUnit.Framework;

namespace Manager.Integration.Test.IntegrationControllerTests
{
	[TestFixture, Ignore]
	public class IntegrationControllerOneManagerAndOneNodeTests : InitialzeAndFinalizeOneManagerAndOneNode
	{
		private void GenerateJobs()
		{
			var createNewJobRequests = JobHelper.GenerateTestJobParamsRequests(10);

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
			var managerIntegrationStopwatch = new ManagerIntegrationStopwatch();

			var taskHlp = startJobTaskHelper.ExecuteCreateNewJobTasks(jobManagerTaskCreators,
			                                                          CancellationTokenSource,
			                                                          timeout);

			checkJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

			var elapsedTime =
				managerIntegrationStopwatch.GetTotalElapsedTimeInSeconds();

			Assert.IsTrue(checkJobHistoryStatusTimer.Guids.Count == createNewJobRequests.Count,
			              "Number of requests must be equal.");

			Assert.IsTrue(checkJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.SuccessStatus));

			CancellationTokenSource.Cancel();
			taskHlp.Dispose();

			foreach (var jobManagerTaskCreator in jobManagerTaskCreators)
			{
				jobManagerTaskCreator.Dispose();
			}
		}

		[Test, Ignore]
		public void ShouldBeAbleToCreateASuccessJobRequestTest()
		{
			var tasks = new List<Task>();

			for (var i = 0; i < 6; i++)
			{
				ShouldBeAbleToStartNewManager();
				ShouldBeAbleToStartNewNode();
			}

			Parallel.For(1, 10, i => { tasks.Add(Task.Factory.StartNew(GenerateJobs)); });

			tasks.Add(Task.Factory.StartNew(async () =>
			{
				await IntegrationControllerApiHelper.ShutDownNode(new IntergrationControllerUriBuilder(), 
																  new HttpSender(), 
																  "Node1.config");

				await IntegrationControllerApiHelper.ShutDownManager(new IntergrationControllerUriBuilder(), 
																	 new HttpSender(), 
																	 "Manager1.config");
			}));

			Parallel.For(1, 10, i => { tasks.Add(Task.Factory.StartNew(GenerateJobs)); });

			tasks.Add(Task.Factory.StartNew(() => IntegrationControllerApiHelper.StartNewManager(new IntergrationControllerUriBuilder(), 
																							     new HttpSender())));

			tasks.Add(Task.Factory.StartNew(() => IntegrationControllerApiHelper.StartNewManager(new IntergrationControllerUriBuilder(), 
																								 new HttpSender())));

			Task.WaitAll(tasks.ToArray());
		}

		[Test, Ignore]
		public void ShouldBeAbleToShutDownManager()
		{
			var intergrationControllerUriBuilder = new IntergrationControllerUriBuilder();
			var httpSender = new HttpSender();

			var nodeName = IntegrationControllerApiHelper.ShutDownManager(intergrationControllerUriBuilder,
			                                                              httpSender,
			                                                              "Manager1.config");

			Assert.IsNotNull(nodeName, "Should shut down manager.");
		}

		[Test, Ignore]
		public void ShouldBeAbleToShutDownNode()
		{
			var intergrationControllerUriBuilder = new IntergrationControllerUriBuilder();
			var httpSender = new HttpSender();

			var nodeName = IntegrationControllerApiHelper.ShutDownNode(intergrationControllerUriBuilder,
			                                                           httpSender,
			                                                           "Node1.config");

			Assert.IsNotNull(nodeName, "Should shut down node.");
		}

		[Test]
		public void ShouldBeAbleToStartNewManager()
		{
			var intergrationControllerUriBuilder = new IntergrationControllerUriBuilder();
			var httpSender = new HttpSender();

			var managerName = IntegrationControllerApiHelper.StartNewManager(intergrationControllerUriBuilder,
			                                                                 httpSender);

			Assert.IsNotNull(managerName, "Should start up a new manager.");
		}

		[Test]
		public void ShouldBeAbleToStartNewNode()
		{
			var intergrationControllerUriBuilder = new IntergrationControllerUriBuilder();
			var httpSender = new HttpSender();

			var nodeName = IntegrationControllerApiHelper.StartNewNode(intergrationControllerUriBuilder,
			                                                           httpSender);

			Assert.IsNotNull(nodeName, "Should start up a new node.");
		}

		[Test]
		public void ShouldHaveStartedOneManager()
		{
			var intergrationControllerUriBuilder = new IntergrationControllerUriBuilder();
			var httpSender = new HttpSender();

			// Get Managers.
			var allManagers = IntegrationControllerApiHelper.GetAllManagers(intergrationControllerUriBuilder,
			                                                                httpSender);

			Assert.IsTrue(allManagers.Result != null);
		}


		[Test]
		public void ShouldHaveStartedOneNode()
		{
			var intergrationControllerUriBuilder = new IntergrationControllerUriBuilder();
			var httpSender = new HttpSender();

			// Get Nodes.
			var allNodes = IntegrationControllerApiHelper.GetAllNodes(intergrationControllerUriBuilder,
			                                                          httpSender);

			Assert.IsTrue(allNodes.Result != null);
		}
	}
}