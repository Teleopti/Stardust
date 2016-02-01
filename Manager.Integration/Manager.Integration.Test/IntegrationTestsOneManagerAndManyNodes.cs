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
    public class IntegrationTestsOneManagerAndManyNodes
    {
        private const int NumberOfNodesToStart = 3;

        private bool _startUpManagerAndNodeManually = false;
        private bool _clearDatabase = true;

        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (IntegrationTestsOneManagerAndManyNodes));

        private Process StartManagerIntegrationConsoleHostProcess { get; set; }

        [SetUp]
        public void SetUp()
        {

#if (DEBUG)
            // Do nothing.
#else
            _clearDatabase= true;
            _startUpManagerAndNodeManually = false;
#endif

            if (_clearDatabase)
            {
                DatabaseHelper.TryClearDatabase();
            }

            ManagerApiHelper = new ManagerApiHelper();

            if (!_startUpManagerAndNodeManually)
            {
                ProcessHelper.ShutDownAllManagerIntegrationConsoleHostProcesses();

                StartManagerIntegrationConsoleHostProcess =
                    ProcessHelper.StartManagerIntegrationConsoleHostProcess(NumberOfNodesToStart);
            }
        }

        private ManagerApiHelper ManagerApiHelper { get; set; }

        [Test]
        public void Create10RequestShouldReturnBothCancelAndDeleteStatuses()
        {
            JobHelper.GiveNodesTimeToInitialize();

            List<JobRequestModel> requests = JobHelper.GenerateLongRunningParamsRequests(10);

            List<Task> tasks = new List<Task>();

            foreach (var jobRequestModel in requests)
            {
                tasks.Add(ManagerApiHelper.CreateManagerDoThisTask(jobRequestModel));

                Logger.Debug("Created task for add job :" + jobRequestModel.Name);
            }

            ManagerApiHelper.CheckJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(requests.Count,
                                                                                         5000,
                                                                                         StatusConstants.CanceledStatus,
                                                                                         StatusConstants.DeletedStatus,
                                                                                         StatusConstants.SuccessStatus,
                                                                                         StatusConstants.FailedStatus);

            ManagerApiHelper.CheckJobHistoryStatusTimer.GuidAddedEventHandler += (sender,
                                                                                  args) =>
            {
                var cancelJobTask = ManagerApiHelper.CreateManagerCancelTask(args.Guid);

                Logger.Debug("IntegrationTestsOneManagerAndManyNodes : Created task for cancel job :" + args.Guid);
                Thread.Sleep(TimeSpan.FromMilliseconds(250));
                cancelJobTask.Start();
            };

            ManagerApiHelper.CheckJobHistoryStatusTimer.Start();

            Parallel.ForEach(tasks,
                             task => { task.Start(); });

            // Time out after 60 seconds.
            ManagerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait(TimeSpan.FromSeconds(60));

            ProcessHelper.CloseProcess(StartManagerIntegrationConsoleHostProcess);

            Assert.IsTrue(ManagerApiHelper.CheckJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.CanceledStatus ||
                                                                                        pair.Value == StatusConstants.DeletedStatus));
        }

        [Test]
        public void JobShouldHaveStatusFailedIfFailed()
        {
            JobHelper.GiveNodesTimeToInitialize();

            List<JobRequestModel> requests = JobHelper.GenerateFailingJobParamsRequests(1);

            List<Task> tasks = new List<Task>();

            foreach (var jobRequestModel in requests)
            {
                tasks.Add(ManagerApiHelper.CreateManagerDoThisTask(jobRequestModel));
            }

            ManagerApiHelper.CheckJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(requests.Count,
                                                                                         5000,
                                                                                         StatusConstants.SuccessStatus,
                                                                                         StatusConstants.DeletedStatus,
                                                                                         StatusConstants.FailedStatus,
                                                                                         StatusConstants.CanceledStatus);

            ManagerApiHelper.CheckJobHistoryStatusTimer.Start();

            Parallel.ForEach(tasks,
                             task => { task.Start(); });

            ManagerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait(TimeSpan.FromSeconds(60)); // If not succeded in 1 min, something is wrong

            Assert.IsTrue(ManagerApiHelper.CheckJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.FailedStatus));

            ProcessHelper.CloseProcess(StartManagerIntegrationConsoleHostProcess);
        }

        [Test]
        [Ignore]
        public void CancelWrongJob()
        {
            //JobHelper.GiveNodesTimeToInitialize(5);

            //Task<HttpResponseMessage> task = ManagerApiHelper.CreateManagerCancelTask(Guid.NewGuid());

            //task.Start();

            //task.Wait();
        }

        [Test]
        public void ShouldBeAbleToCreate10SuccessJobRequest()
        {
            JobHelper.GiveNodesTimeToInitialize();

            List<JobRequestModel> requests = JobHelper.GenerateTestJobParamsRequests(10);

            List<Task> tasks = new List<Task>();

            foreach (var jobRequestModel in requests)
            {
                tasks.Add(ManagerApiHelper.CreateManagerDoThisTask(jobRequestModel));
            }

            ManagerApiHelper.CheckJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(requests.Count,
                                                                                         5000,
                                                                                         StatusConstants.SuccessStatus,
                                                                                         StatusConstants.DeletedStatus,
                                                                                         StatusConstants.FailedStatus,
                                                                                         StatusConstants.CanceledStatus);

            ManagerApiHelper.CheckJobHistoryStatusTimer.Start();

            Parallel.ForEach(tasks,
                             task => { task.Start(); });

            ManagerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait(TimeSpan.FromSeconds(60)); // If not succeded in 1 min, something is wrong

            Assert.IsTrue(ManagerApiHelper.CheckJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.SuccessStatus));

            ProcessHelper.CloseProcess(StartManagerIntegrationConsoleHostProcess);
        }
    }
}