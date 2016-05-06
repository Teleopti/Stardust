using System;
using System.Linq;
using System.Threading;
using Manager.Integration.Test.Database;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Initializers;
using Manager.Integration.Test.Timers;
using NUnit.Framework;

namespace Manager.Integration.Test.Tests.FunctionalTests
{
	[TestFixture]
	public class OneManagerAndOneNodeTests : InitialzeAndFinalizeOneManagerAndOneNodeWait
	{
		[Test]
		public void CancelWrongJobsTest()
		{
			var startedTest = DateTime.UtcNow;
			var manualResetEventSlim = new ManualResetEventSlim();
			
			var checkTablesInManagerDbTimer =
				new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 100);

			checkTablesInManagerDbTimer.ReceivedJobItem += (sender, items) =>
			{
				if (items.Any() &&
				    items.All(job => job.Ended == null))
				{
					HttpRequestManager.CancelJob(Guid.NewGuid());
				}
				if (items.Any() &&
					items.All(job => job.Ended != null))
				{
					manualResetEventSlim.Set();
				}
			};
			checkTablesInManagerDbTimer.JobTimer.Start();


			var jobQueueItem =
				JobHelper.GenerateTestJobTimerRequests(1, TimeSpan.FromSeconds(5)).First();
			var jobId = HttpRequestManager.AddJob(jobQueueItem);
			

			manualResetEventSlim.Wait(TimeSpan.FromSeconds(20));

			Assert.IsTrue(!checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any(), "Job queue must be empty.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any(), "Jobs must have been added.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.All(job => job.Result.StartsWith("Success", StringComparison.InvariantCultureIgnoreCase)));

			checkTablesInManagerDbTimer.Dispose();

			var endedTest = DateTime.UtcNow;

			var description =
				string.Format("Creates CANCEL WRONG jobs with {0} manager and {1} nodes.",
				              NumberOfManagers,
				              NumberOfNodes);

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
			                                  description,
			                                  startedTest,
			                                  endedTest);
		}

		[Test]
		public void CreateRequestShouldReturnCancelStatusWhenJobHasStartedAndBeenCanceled()
		{
			var startedTest = DateTime.UtcNow;
			var manualResetEventSlim = new ManualResetEventSlim();
			

			var checkTablesInManagerDbTimer = new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 100);
			checkTablesInManagerDbTimer.ReceivedJobItem += (sender, items) =>
			{
				if (items.Any() &&
					items.All(job => job.Ended == null))
				{
					HttpRequestManager.CancelJob(items.First().JobId);
				}
				if (items.Any() &&
					items.All(job => job.Ended != null))
				{
					manualResetEventSlim.Set();
				}
			};
			checkTablesInManagerDbTimer.JobTimer.Start();
			

			var jobQueueItem =
				JobHelper.GenerateTestJobTimerRequests(1, TimeSpan.FromMinutes(1)).First();
			var jobId = HttpRequestManager.AddJob(jobQueueItem);


			manualResetEventSlim.Wait(TimeSpan.FromSeconds(30));

			Assert.IsTrue(!checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any(), "Job queue must be empty.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any(), "Jobs must have been added.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any(job => job.Result.StartsWith("Canceled", StringComparison.InvariantCultureIgnoreCase)));

			checkTablesInManagerDbTimer.Dispose();

			var endedTest = DateTime.UtcNow;

			var description =
				string.Format("Creates Cancel Job with {0} manager and {1} nodes.",
				              NumberOfManagers,
				              NumberOfNodes);

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
			                                  description,
			                                  startedTest,
			                                  endedTest);
		}


		[Test]
		public void JobShouldHaveStatusFailedIfFailedTest()
		{
			var startedTest = DateTime.UtcNow;
			var manualResetEventSlim = new ManualResetEventSlim();

			
			var checkTablesInManagerDbTimer =
				new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 100);

			checkTablesInManagerDbTimer.ReceivedJobItem += (sender, items) =>
			{
				if (items.Any() &&
				    items.All(job => job.Ended != null))
				{
					if (!manualResetEventSlim.IsSet)
					{
						manualResetEventSlim.Set();
					}
				}
			};
			checkTablesInManagerDbTimer.JobTimer.Start();


			var failingJobQueueItem =
				JobHelper.GenerateFailingJobParamsRequests(1).First();
			var jobId = HttpRequestManager.AddJob(failingJobQueueItem);


			manualResetEventSlim.Wait(TimeSpan.FromSeconds(10));

			Assert.IsTrue(!checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any(), "Job queue must be empty.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any(), "Jobs must have been added.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.
				              Jobs.Any(job => job.Result.StartsWith("fail", StringComparison.InvariantCultureIgnoreCase)));

			var endedTest = DateTime.UtcNow;

			var description =
				string.Format("Creates FAILED jobs with {0} manager and {1} nodes.",
				              NumberOfManagers,
				              NumberOfNodes);

			DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
			                                  description,
			                                  startedTest,
			                                  endedTest);


			checkTablesInManagerDbTimer.Dispose();
		}

		/// <summary>
		///     DO NOT FORGET TO RUN COMMAND BELOW AS ADMINISTRATOR.
		///     netsh http add urlacl url=http://+:9050/ user=everyone listen=yes
		/// </summary>
		[Test]
		public void ShouldBeAbleToCreateASuccessJobRequestTest()
		{
			var startedTest = DateTime.UtcNow;
			var manualResetEventSlim = new ManualResetEventSlim();
			

			var checkTablesInManagerDbTimer =
				new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 2000);

			checkTablesInManagerDbTimer.ReceivedJobItem += (sender, items) =>
			{
				if (items.Any() &&
				    items.All(job => job.Started != null && job.Ended != null))
				{
					manualResetEventSlim.Set();
				}
			};
			checkTablesInManagerDbTimer.JobTimer.Start();


			var jobQueueItem =
				JobHelper.GenerateTestJobTimerRequests(1, TimeSpan.FromSeconds(5)).First();
			var jobId = HttpRequestManager.AddJob(jobQueueItem);
			

			manualResetEventSlim.Wait(TimeSpan.FromSeconds(30));

			Assert.IsTrue(!checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any(), "Should not be any jobs left in queue.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any(), "There should be jobs in jobs table.");
			Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.
				              Jobs.All(job => job.Result.StartsWith("success", StringComparison.InvariantCultureIgnoreCase)));

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