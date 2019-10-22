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
using NodeTest.JobHandlers;
using NUnit.Framework;

namespace Manager.Integration.Test.Tests.RecoveryTests
{
	[TestFixture]
	public class RecoverFromNodeCrashTest : InitializeAndFinalizeOneManagerAndNodes
	{
		[Test]
		public void GoodNameLater()
        {
            WaitForNodeToFinishWorking();
            var startedTest = DateTime.UtcNow;
            var manualResetEventSlim = new ManualResetEventSlim();
            var checkTablesInManagerDbTimer =
                new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 2000);


            checkTablesInManagerDbTimer.GetJobItems += (sender, items) =>
            {
                if (items.Any() &&
                    items.All(job => job.Started != null && job.Ended != null))
                {
                    manualResetEventSlim.Set();

                }
            };
            checkTablesInManagerDbTimer.JobTimer.Start();

            // ############## JOB SETUP #################
            var crashingJobParams = new CrashingJobParams("Error message - THIS SHOULD CRASH");
            var crashingJobParamsJson = JsonConvert.SerializeObject(crashingJobParams);
            var crashingJob = new  JobQueueItem
            {
                Name = "Crashing Job",
                Serialized = crashingJobParamsJson,
                Type = nameof(CrashingJobParams),
                CreatedBy = "Test"
            };
            var jobId = HttpRequestManager.AddJob(crashingJob);
            // #################################

            //var jobQueueItem =
            //    JobHelper.GenerateTestJobRequests(1, 5).First();
            //var jobId = HttpRequestManager.AddJob(jobQueueItem);
			

            manualResetEventSlim.Wait(TimeSpan.FromSeconds(30));

            Assert.IsTrue(!checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any(), "Should not be any jobs left in queue.");
            Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any(), "There should be jobs in jobs table.");
            Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.
                Jobs.All(job => job.Result.StartsWith("success", StringComparison.InvariantCultureIgnoreCase)));

            checkTablesInManagerDbTimer.Dispose();

            var endedTest = DateTime.UtcNow;

            var description =
                $"Creates {1} Test Timer jobs with {NumberOfManagers} manager and {NumberOfNodes} nodes.";

            DatabaseHelper.AddPerformanceData(ManagerDbConnectionString,
                description,
                startedTest,
                endedTest);
        }




        private void WaitForNodeToStartWorking()
        {
            var working = false;
            while (!working)
            {
                working = HttpRequestManager.IsNodeWorking();
                Thread.Sleep(TimeSpan.FromMilliseconds(200));
            }
        }

        private void WaitForNodeToFinishWorking()
        {
            var working = true;
            while (working)
            {
                working = HttpRequestManager.IsNodeWorking();
                Thread.Sleep(TimeSpan.FromMilliseconds(200));
            }
        }

       
	}
}