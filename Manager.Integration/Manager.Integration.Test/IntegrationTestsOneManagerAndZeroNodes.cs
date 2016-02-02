using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Manager.Integration.Test.Constants;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Timers;
using NUnit.Framework;

namespace Manager.Integration.Test
{
    [TestFixture]
    public class IntegrationTestsOneManagerAndZeroNodes
    {

        [SetUp]
        public void SetUp()
        {
            XmlConfigurator.Configure();

#if (DEBUG)
            // Do nothing.
#else
            _clearDatabase = true;
            _startUpManagerAndNodeManually = false;
#endif
            XmlConfigurator.Configure();

            if (_clearDatabase)
            {
                DatabaseHelper.TryClearDatabase();
            }

            ManagerApiHelper = new ManagerApiHelper();

            ProcessHelper.ShutDownAllManagerAndNodeProcesses();
       //     ProcessHelper.ShutDownAllProcesses("Manager.IntegrationTest.Console.Host");

            if (_startUpManagerAndNodeManually)
            {
                ProcessHelper.ShutDownAllManagerIntegrationConsoleHostProcesses();
            }
            else
            {
                StartManagerIntegrationConsoleHostProcess =
                    ProcessHelper.StartManagerIntegrationConsoleHostProcess(NumberOfNodesToStart);
            }

        }

        [TearDown]
        public void Teardown()
        {
            ProcessHelper.ShutDownAllProcesses();
        }


        private static readonly ILog Logger = LogManager.GetLogger(typeof(IntegrationTestsOneManagerAndZeroNodes));

        private Process StartManagerIntegrationConsoleHostProcess { get; set; }

        private const int NumberOfNodesToStart = 0;

        private bool _startUpManagerAndNodeManually = false;
        private bool _clearDatabase = true;

        private ManagerApiHelper ManagerApiHelper { get; set; }

        [Test]
        public void JobShouldJustBeQueuedIfNoNodes()
        {
            JobHelper.GiveNodesTimeToInitialize();

            List<JobRequestModel> requests = JobHelper.GenerateTestJobParamsRequests(1);

            var timeout = JobHelper.GenerateTimeoutTimeInSeconds(requests.Count,30);

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

            ManagerApiHelper.CheckJobHistoryStatusTimer.Start();

            ManagerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

            Assert.IsTrue(ManagerApiHelper.CheckJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.NullStatus));
            
        }
    }
}