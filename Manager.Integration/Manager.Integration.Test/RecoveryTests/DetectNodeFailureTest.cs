using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Initializers;
using Manager.Integration.Test.Timers;
using Manager.IntegrationTest.Console.Host.Helpers;
using Manager.IntegrationTest.Console.Host.Log4Net;
using NUnit.Framework;

namespace Manager.Integration.Test.RecoveryTests
{
	[TestFixture]
	internal class DetectNodeFailureTest : InitialzeAndFinalizeOneManagerAndOneNodeWait
	{
		private void LogMessage(string message)
		{
			this.Log().DebugWithLineNumber(message);
		}

		public ManagerUriBuilder MangerUriBuilder { get; set; }

		public HttpSender HttpSender { get; set; }

		public ManualResetEventSlim WaitForJobToStartEvent { get; set; }

		public ManualResetEventSlim WaitForNodeToEndEvent { get; set; }

		public override void SetUp()
		{
			DatabaseHelper.TruncateJobQueueTable(ManagerDbConnectionString);
			DatabaseHelper.TruncateJobTable(ManagerDbConnectionString);
			DatabaseHelper.TruncateJobDetailTable(ManagerDbConnectionString);
		}

		[Test]
		public void ShouldConsiderNodeAsDeadWhenInactiveAndSetJobResultToFatal()
		{
			LogMessage("Start test.");

			var startedTest = DateTime.UtcNow;

			var timeout = TimeSpan.FromMinutes(10);

			WaitForJobToStartEvent = new ManualResetEventSlim();
			WaitForNodeToEndEvent = new ManualResetEventSlim();

			//---------------------------------------------------------
			// Database validator.
			//---------------------------------------------------------
			var checkTablesInManagerDbTimer =
				new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 2000);

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

			checkTablesInManagerDbTimer.JobTimer.Start();
			checkTablesInManagerDbTimer.WorkerNodeTimer.Start();

			//---------------------------------------------------------
			// HTTP Request.
			//---------------------------------------------------------
			HttpSender = new HttpSender();
			MangerUriBuilder = new ManagerUriBuilder();

			var addToJobQueueUri = MangerUriBuilder.GetAddToJobQueueUri();

			var jobQueueItem =
				JobHelper.GenerateTestJobTimerRequests(1, TimeSpan.FromMinutes(10)).First();

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

					Thread.Sleep(TimeSpan.FromSeconds(10));
				}
			}, CancellationTokenSource.Token);

			addToJobQueueTask.Start();
			addToJobQueueTask.Wait(timeout);

			WaitForJobToStartEvent.Wait(timeout);

			Task<string> taskShutDownNode = new Task<string>(() =>
			{
				string res = IntegrationControllerApiHelper.ShutDownNode(new IntergrationControllerUriBuilder(),
																		HttpSender,
																		"Node1.config").Result;

				return res;
			});

			taskShutDownNode.Start();
			taskShutDownNode.Wait();
			
			checkTablesInManagerDbTimer.ReceivedWorkerNodesData += (sender, nodes) =>
			{
				if (nodes.Any() && nodes.All(node => node.Alive == false))
				{
					if (!WaitForNodeToEndEvent.IsSet)
					{
						WaitForNodeToEndEvent.Set();
					}					
				}
			};

			WaitForNodeToEndEvent.Wait(timeout);

			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.WorkerNodes.All(node => node.Alive == false), "Worker Nodes should not be alive.");
			Assert.IsTrue(!checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any(),"Job queue should be empty.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any(), "Job should not be empty.");

			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.All(job => job.Result.StartsWith("Fatal Node Failure",StringComparison.InvariantCultureIgnoreCase)),
						"All jobs shall have status of Fatal Node Failure.");

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

			LogMessage("Finished test.");
		}
		
	}
}