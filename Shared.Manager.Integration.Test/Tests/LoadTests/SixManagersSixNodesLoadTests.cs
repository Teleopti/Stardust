using System;
using System.Linq;
using System.Threading;
using Manager.Integration.Test.Database;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Initializers;
using Manager.Integration.Test.Timers;
using NUnit.Framework;

namespace Manager.Integration.Test.Tests.LoadTests
{
	[TestFixture]
	internal class SixManagersSixNodesLoadTests : InitializeAndFinalizeSixManagersAndSixNodes
	{
		/// <summary>
		///     DO NOT FORGET TO RUN COMMAND BELOW AS ADMINISTRATOR.
		///     netsh http add urlacl url=http://+:9050/ user=everyone listen=yes
		/// </summary>
		[Test]
		public void ShouldBeAbleToCreateManySuccessJobRequestTest()
		{
			var startedTest = DateTime.UtcNow;
			var manualResetEventSlim = new ManualResetEventSlim();

			var createNewJobRequests =
				JobHelper.GenerateTestJobRequests(50, 1);
			

			var checkTablesInManagerDbTimer =
				new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 2000);

			checkTablesInManagerDbTimer.GetJobItems += (sender, jobs) =>
			{
				if (jobs.Count == createNewJobRequests.Count &&
				    jobs.All(job => job.Started != null && job.Ended != null))
				{
					manualResetEventSlim.Set();
				}
			};
			checkTablesInManagerDbTimer.JobTimer.Start();

			
			foreach (var newJobRequest in createNewJobRequests)
			{
				var jobId = HttpRequestManager.AddJob(newJobRequest);
			}

			manualResetEventSlim.Wait(TimeSpan.FromMinutes(5));

			var jobsToAssert = checkTablesInManagerDbTimer.ManagerDbRepository.Jobs;
			Assert.IsTrue(jobsToAssert.Count == createNewJobRequests.Count);
			Assert.IsTrue(jobsToAssert.All(job => job.Result.StartsWith("Success", StringComparison.InvariantCultureIgnoreCase)));

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
		}
	}
}