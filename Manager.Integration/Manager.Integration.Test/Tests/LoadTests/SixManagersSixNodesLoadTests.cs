using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Manager.Integration.Test.Database;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Initializers;
using Manager.Integration.Test.Timers;
using Manager.IntegrationTest.Console.Host.Helpers;
using Manager.IntegrationTest.Console.Host.Log4Net;
using NUnit.Framework;

namespace Manager.Integration.Test.Tests.LoadTests
{
	[TestFixture]
	internal class SixManagersSixNodesLoadTests : InitialzeAndFinalizeSixManagersAndSixNodes
	{
		private void LogMessage(string message)
		{
			this.Log().DebugWithLineNumber(message);
		}

		public ManualResetEventSlim ManualResetEventSlim { get; set; }

		/// <summary>
		///     DO NOT FORGET TO RUN COMMAND BELOW AS ADMINISTRATOR.
		///     netsh http add urlacl url=http://+:9050/ user=everyone listen=yes
		/// </summary>
		[Test]
		public void ShouldBeAbleToCreateManySuccessJobRequestTest()
		{
			this.Log().DebugWithLineNumber("Start test.");

			var startedTest = DateTime.UtcNow;

			var createNewJobRequests =
				JobHelper.GenerateTestJobTimerRequests(50,
				                                       TimeSpan.FromSeconds(1));

			var timeout =
				JobHelper.GenerateTimeoutTimeInMinutes(createNewJobRequests.Count,
				                                       2);

			//---------------------------------------------------------
			// Database validator.
			//---------------------------------------------------------
			var checkTablesInManagerDbTimer =
				new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 2000);

			checkTablesInManagerDbTimer.ReceivedJobItem += (sender, jobs) =>
			{
				if (jobs.Any() &&
				    jobs.All(job => job.Started != null && job.Ended != null))
				{
					if (!ManualResetEventSlim.IsSet)
					{
						ManualResetEventSlim.Set();
					}
				}
			};

			HttpSender = new HttpSender();
			MangerUriBuilder = new ManagerUriBuilder();
			ManualResetEventSlim = new ManualResetEventSlim();

			var addToJobQueueUri = MangerUriBuilder.GetAddToJobQueueUri();

			var tasks = new List<Task>();

			foreach (var newJobRequest in createNewJobRequests)
			{
				var request = newJobRequest;

				tasks.Add(new Task(() =>
				{
					var numberOfTries = 0;

					while (true)
					{
						numberOfTries++;

						try
						{
							var response =
								HttpSender.PostAsync(addToJobQueueUri, request).Result;

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
				}, CancellationTokenSource.Token));
			}

			checkTablesInManagerDbTimer.JobTimer.Start();

			Parallel.ForEach(tasks, task => { task.Start(); });

			ManualResetEventSlim.Wait(timeout);

			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.All(job => job.Result.StartsWith("success", StringComparison.InvariantCultureIgnoreCase)));

			checkTablesInManagerDbTimer.Dispose();

			var endedTest = DateTime.UtcNow;

			var description =
				string.Format("Creates {0} FAST = 1 second jobs with {1} manager and {2} nodes.",
				              createNewJobRequests.Count,
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