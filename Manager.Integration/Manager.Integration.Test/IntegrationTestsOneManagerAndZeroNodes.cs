using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Manager.Integration.Test.Constants;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Timers;
using NUnit.Framework;

namespace Manager.Integration.Test
{
    [TestFixture]
    public class IntegrationTestsOneManagerAndZeroNodes
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (IntegrationTestsOneManagerAndZeroNodes));

        private const int NumberOfNodesToStart = 0;

        [SetUp]
        public void Setup()
        {
            DatabaseHelper.TryClearDatabase();

            ManagerApiHelper = new ManagerApiHelper();

            ProcessHelper.ShutDownAllManagerIntegrationConsoleHostProcesses();

            StartManagerIntegrationConsoleHostProcess =
                ProcessHelper.StartManagerIntegrationConsoleHostProcess(NumberOfNodesToStart);

            DatabaseHelper.TryClearDatabase();
        }

        private ManagerApiHelper ManagerApiHelper { get; set; }

        [Test]
        public void JobShouldJustBeQueuedIfNoNodes()
        {
            List<JobRequestModel> requests = JobHelper.GenerateTestJobParamsRequests(1);

            List<Task> tasks = new List<Task>();

            foreach (var jobRequestModel in requests)
            {
                tasks.Add(ManagerApiHelper.CreateManagerDoThisTask(jobRequestModel));
            }

            ManagerApiHelper.CheckJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(requests.Count,
                                                                                         5000,
                                                                                         StatusConstants.NullStatus, 
                                                                                         StatusConstants.EmptyStatus);

            Parallel.ForEach(tasks,
                             task => { task.Start(); });
            
            Thread.Sleep(TimeSpan.FromSeconds(5));  //short sleep, shouldnt crash

            ProcessHelper.CloseProcess(StartManagerIntegrationConsoleHostProcess);

            Assert.IsTrue(
                ManagerApiHelper.CheckJobHistoryStatusTimer.Guids.All(
                    pair => pair.Value == StatusConstants.NullStatus));

        }

        private Process StartManagerIntegrationConsoleHostProcess { get; set; }
    }
}