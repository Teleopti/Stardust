using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        [Test][Ignore]
        public void TryStartOneJob()
        {
            List<JobRequestModel> requests = JobHelper.GenerateTestJobParamsRequests(5);

            List<Task> tasks = new List<Task>();

            foreach (var jobRequestModel in requests)
            {
                tasks.Add(ManagerApiHelper.CreateManagerDoThisTask(jobRequestModel));
            }

            ManagerApiHelper.CheckJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(requests.Count,
                                                                                         5000,
                                                                                         StatusConstants.NullStatus, 
                                                                                         StatusConstants.EmptyStatus);
            ManagerApiHelper.CheckJobHistoryStatusTimer.Start();

            Parallel.ForEach(tasks,
                             task => { task.Start(); });

            ManagerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait();

            ProcessHelper.CloseProcess(StartManagerIntegrationConsoleHostProcess);

            var numberOfStatuses =
                ManagerApiHelper.CheckJobHistoryStatusTimer.Guids.Values.Where(s => s == StatusConstants.NullStatus || s == StatusConstants.EmptyStatus)
                    .ToList()
                    .Count;

            Assert.IsTrue(ManagerApiHelper.CheckJobHistoryStatusTimer.Guids.Keys.Count == numberOfStatuses);
        }

        private Process StartManagerIntegrationConsoleHostProcess { get; set; }
    }
}