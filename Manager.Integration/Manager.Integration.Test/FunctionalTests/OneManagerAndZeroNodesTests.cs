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

namespace Manager.Integration.Test.FunctionalTests
{
	[TestFixture]
	public class OneManagerAndZeroNodesTests : InitialzeAndFinalizeOneManagerAndZeroNodes
	{
		public ManualResetEventSlim ManualResetEventSlim { get; set; }

		public ManagerUriBuilder MangerUriBuilder { get; set; }

		public HttpSender HttpSender { get; set; }

		[Test]
		public void JobsShouldJustBeQueuedIfNoNodesTest()
		{
			this.Log().DebugWithLineNumber("Start test.");

			var startedTest = DateTime.UtcNow;

			var timeout = TimeSpan.FromMinutes(5);
			ManualResetEventSlim = new ManualResetEventSlim();

			//---------------------------------------------------------
			// Database validator.
			//---------------------------------------------------------
			var checkTablesInManagerDbTimer =
				new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 2000);

			checkTablesInManagerDbTimer.ReceivedJobQueueItem += (sender, items) =>
			{
				if (items.Any())
				{
					if (!ManualResetEventSlim.IsSet)
					{
						ManualResetEventSlim.Set();
					}
				}
			};

			checkTablesInManagerDbTimer.JobQueueTimer.Start();

			//---------------------------------------------------------
			// HTTP Request.
			//---------------------------------------------------------
			HttpSender = new HttpSender();
			MangerUriBuilder = new ManagerUriBuilder();

			var addToJobQueueUri = MangerUriBuilder.GetAddToJobQueueUri();

			var jobQueueItem =
				JobHelper.GenerateTestJobTimerRequests(1, TimeSpan.FromSeconds(5)).First();

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

			ManualResetEventSlim.Wait(timeout);

			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any());
			Assert.IsTrue(!checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any());

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

			this.Log().DebugWithLineNumber("Finished test.");
		}
	}
}