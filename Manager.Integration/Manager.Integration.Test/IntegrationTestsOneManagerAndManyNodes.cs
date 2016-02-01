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
        private const int NumberOfNodesToStart = 1;

        private readonly bool _startUpManagerAndNodeManually = false;
        private readonly bool _clearDatabase= true;

        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (IntegrationTestsOneManagerAndManyNodes));

        private Process StartManagerIntegrationConsoleHostProcess { get; set; }

        [SetUp]
        public void SetUp()
        {
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
        public void Create2RequestShouldReturnBothCancelAndDeleteStatuses()
        {
            JobHelper.GiveNodesTimeToInitialize();

            List<JobRequestModel> requests = JobHelper.GenerateLongRunningParamsRequests(1);

            List<Task> tasks = new List<Task>();

            foreach (var jobRequestModel in requests)
            {
                tasks.Add(ManagerApiHelper.CreateManagerDoThisTask(jobRequestModel));

                Logger.Debug("Created task for add job :" + jobRequestModel.Name);
            }

            ManagerApiHelper.CheckJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(requests.Count,
                                                                                         5000,
                                                                                         StatusConstants.CanceledStatus,
                                                                                         StatusConstants.DeletedStatus);

            ManagerApiHelper.CheckJobHistoryStatusTimer.GuidAddedEventHandler += (sender,
                                                                                  args) =>
            {
                var cancelJobTask = ManagerApiHelper.CreateManagerCancelTask(args.Guid);

                Logger.Debug("IntegrationTestsOneManagerAndManyNodes : Created task for cancel job :" + args.Guid);

                cancelJobTask.Start();
            };

            ManagerApiHelper.CheckJobHistoryStatusTimer.Start();

            Parallel.ForEach(tasks,
                             task => { task.Start(); });

            ManagerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait();

            ProcessHelper.CloseProcess(StartManagerIntegrationConsoleHostProcess);

            Assert.IsTrue(ManagerApiHelper.CheckJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.CanceledStatus ||
                                                                                            pair.Value == StatusConstants.DeletedStatus));
        }

        [Test]
        [Ignore]
        public void FailJobTest()
        {
            string status = string.Empty;

            JobHelper.GiveNodesTimeToInitialize(5);

            List<JobRequestModel> requests = JobHelper.GenerateFailingJobParamsRequests(1);
            List<Task> tasks = new List<Task>();

            foreach (var jobRequestModel in requests)
            {
                tasks.Add(ManagerApiHelper.CreateManagerDoThisTask(jobRequestModel));
            }

            ManagerApiHelper.CheckJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(requests.Count,
                                                                                         5000,
                                                                                         StatusConstants.FailedStatus);

            ManagerApiHelper.CheckJobHistoryStatusTimer.GuidStatusChangedEvent += (sender,
                                                                                   args) =>
            { status = args.NewStatus; };

            ManagerApiHelper.CheckJobHistoryStatusTimer.Start();

            Parallel.ForEach(tasks,
                             task => { task.Start(); });

            ManagerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait();

            Assert.AreEqual(StatusConstants.FailedStatus,
                            status);

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
        public void ShouldBeAbleToCreate1SuccessJobRequest()
        {
            JobHelper.GiveNodesTimeToInitialize();

            List<JobRequestModel> requests = JobHelper.GenerateTestJobParamsRequests(1);

            List<Task> tasks = new List<Task>();

            foreach (var jobRequestModel in requests)
            {
                tasks.Add(ManagerApiHelper.CreateManagerDoThisTask(jobRequestModel));
            }

            ManagerApiHelper.CheckJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(requests.Count,
                                                                                         5000,
                                                                                         StatusConstants.SuccessStatus);

            ManagerApiHelper.CheckJobHistoryStatusTimer.GuidStatusChangedEvent += (sender,
                                                                                   args) =>
            { };

            ManagerApiHelper.CheckJobHistoryStatusTimer.Start();

            Parallel.ForEach(tasks,
                             task => { task.Start(); });

            ManagerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait();

            Assert.IsTrue(ManagerApiHelper.CheckJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.SuccessStatus));

            ProcessHelper.CloseProcess(StartManagerIntegrationConsoleHostProcess);
        }
    }
}