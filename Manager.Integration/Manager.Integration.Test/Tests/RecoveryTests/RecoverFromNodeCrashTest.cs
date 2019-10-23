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
            var waitForAllJobsDone = new ManualResetEventSlim();
            var checkTablesInManagerDbTimer =
                new CheckTablesInManagerDbTimer(ManagerDbConnectionString, 2000);
            
            checkTablesInManagerDbTimer.GetJobItems += (sender, items) =>
            {
                if (items.Any() &&
                    items.All(job => job.Started != null && job.Ended != null))
                {
                    waitForAllJobsDone.Set();

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
                Type = typeof(CrashingJobParams).ToString(),
                CreatedBy = "Test"
            };
            
            var jobId = HttpRequestManager.AddJob(crashingJob);
            // #################################

            var waitResult = waitForAllJobsDone.Wait(TimeSpan.FromSeconds(120));
            
            //Assert.IsTrue(!checkTablesInManagerDbTimer.ManagerDbRepository.JobQueueItems.Any(), "Should not be any jobs left in queue.");
            //Assert.IsTrue(checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.Any(), "There should be jobs in jobs table.");
            Assert.AreEqual("Failed", checkTablesInManagerDbTimer.ManagerDbRepository.Jobs.SingleOrDefault(j => j.JobId == jobId).Result);

            var allNodesAreIdle = WaitForNodeToFinishWorking(TimeSpan.FromSeconds(5));
            Assert.IsTrue(allNodesAreIdle);
            
            checkTablesInManagerDbTimer.Dispose();
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

        private bool WaitForNodeToFinishWorking(TimeSpan? timeSpan = null)
        {
            var startedWaiting = DateTime.Now;

            var working = true;
            while (working)
            {
                working = HttpRequestManager.IsNodeWorking();
                Thread.Sleep(TimeSpan.FromMilliseconds(200));
                if (timeSpan != null && DateTime.Now > startedWaiting.Add(timeSpan.Value))
                    return false;
            }
            return true;
        }
    }
}