﻿using System;
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
	public class OneManagerAndZeroNodesTests : InitialzeAndFinalizeOneManagerAndZeroNodes
	{
		[Test]
		public void JobsShouldJustBeQueuedIfNoNodesTest()
		{
			var startedTest = DateTime.UtcNow;

			var manualResetEventSlim = new ManualResetEventSlim();
			

			var checkTablesInManagerDbTimer =
				new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 100);

			checkTablesInManagerDbTimer.GetJobQueueItems += (sender, items) =>
			{
				if (items.Any())
				{
					manualResetEventSlim.Set();
				}
			};
			checkTablesInManagerDbTimer.JobQueueTimer.Start();
			
			var jobQueueItem =
				JobHelper.GenerateTestJobRequests(1, TimeSpan.FromSeconds(5)).First();
			HttpRequestManager.AddJob(jobQueueItem);

			
			manualResetEventSlim.Wait(TimeSpan.FromSeconds(30));
			//Wait a little time to make sure it is only queued and not sent to node
			Thread.Sleep(TimeSpan.FromSeconds(5));

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
		}
	}
}