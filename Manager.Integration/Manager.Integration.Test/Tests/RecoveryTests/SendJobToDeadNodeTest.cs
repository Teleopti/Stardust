using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Manager.Integration.Test.Database;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Initializers;
using Manager.Integration.Test.Timers;
using Manager.IntegrationTest.Console.Host.Helpers;
using NUnit.Framework;

namespace Manager.Integration.Test.Tests.RecoveryTests
{
	[TestFixture]
	class SendJobToDeadNodeTest : InitialzeAndFinalizeOneManagerAndOneNodeWait
	{

		public ManagerUriBuilder MangerUriBuilder { get; set; }

		public HttpSender HttpSender { get; set; }

		public ManualResetEventSlim WaitForJobToStartEvent { get; set; }
		public ManualResetEventSlim WaitForNodeToStartEvent { get; set; }

		public override void SetUp()
		{
			DatabaseHelper.TruncateJobQueueTable(ManagerDbConnectionString);
			DatabaseHelper.TruncateJobTable(ManagerDbConnectionString);
			DatabaseHelper.TruncateJobDetailTable(ManagerDbConnectionString);
		}

		[Test]
		public void ShouldSendAJobToTheNextNodeIfTheFirstIsNotResponding()
		{
			var startedTest = DateTime.UtcNow;

			WaitForJobToStartEvent = new ManualResetEventSlim();
			WaitForNodeToStartEvent = new ManualResetEventSlim();

			//---------------------------------------------------------
			// Database validators.
			//---------------------------------------------------------
			var checkTablesInManagerDbTimer =
				new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 500);

			checkTablesInManagerDbTimer.ReceivedJobItem += (sender, items) =>
			{
				if (items.Any() && items.All(job => job.Started != null))
				{
					if (!WaitForJobToStartEvent.IsSet)
					{
						WaitForJobToStartEvent.Set();
					}
				}
			};

			checkTablesInManagerDbTimer.ReceivedWorkerNodesData += (sender, nodes) =>
			{
				if (nodes.Count == 2)
				{
					if (!WaitForNodeToStartEvent.IsSet)
					{
						WaitForNodeToStartEvent.Set();
					}
				}
			};

			checkTablesInManagerDbTimer.JobTimer.Start();
			checkTablesInManagerDbTimer.WorkerNodeTimer.Start();

			//---------------------------------------------------------
			// Start Second Node
			//---------------------------------------------------------
			HttpSender = new HttpSender();

			Task<string> taskStartNewNode = new Task<string>(() =>
			{
				string res = IntegrationControllerApiHelper.StartNewNode(new IntergrationControllerUriBuilder(), HttpSender).Result;

				return res;
			});

			taskStartNewNode.Start();

			WaitForNodeToStartEvent.Wait();


			//---------------------------------------------------------
			// Kill Second Node
			//---------------------------------------------------------

			Task<string> taskShutDownNode = new Task<string>(() =>
			{
				string res = IntegrationControllerApiHelper.ShutDownNode(new IntergrationControllerUriBuilder(),
																		HttpSender,
																		"Node2.config").Result;

				return res;
			});

			taskShutDownNode.Start();

			//---------------------------------------------------------
			// Post Job to Manager
			//---------------------------------------------------------
			MangerUriBuilder = new ManagerUriBuilder();

			var addToJobQueueUri = MangerUriBuilder.GetAddToJobQueueUri();

			var jobQueueItem =
				JobHelper.GenerateTestJobTimerRequests(1, TimeSpan.FromSeconds(2)).First();

			CancellationTokenSource = new CancellationTokenSource();

			var addToJobQueueTask = new Task(() =>
			{
				var numberOfTries = 0;

				while (true)
				{
					numberOfTries++;

					try
					{
						var response =
							HttpSender.PostAsync(addToJobQueueUri, jobQueueItem).Result;

						if (response.IsSuccessStatusCode || numberOfTries == 10)
						{
							return;
						}
					}

					catch (AggregateException aggregateException)
					{
						if (aggregateException.InnerException is HttpRequestException)
						{
							// try again.
						}
					}

					Thread.Sleep(TimeSpan.FromSeconds(1));
				}
			}, CancellationTokenSource.Token);

			addToJobQueueTask.Start();
			addToJobQueueTask.Wait();

			//Should not take long time to assign job, if it takes more then some seconds 
			// then it is maybe the background timer who assigned the job
			WaitForJobToStartEvent.Wait(TimeSpan.FromSeconds(10));

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
