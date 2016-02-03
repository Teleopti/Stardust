using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
    public class IntegrationTestsOneManagerAndManyNodes
    {
        [SetUp]
        public void Setup()
        {
            if (_clearDatabase)
            {
                DatabaseHelper.TryClearDatabase();
            }
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {

            var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));


#if (DEBUG)
            // Do nothing.
#else
            _clearDatabase = false;
            _startUpManagerAndNodeManually = false;
            _debugMode = false;
            _buildMode = "Release";
#endif

            ManagerApiHelper = new ManagerApiHelper();

            if (!_startUpManagerAndNodeManually)
            {
                if (_debugMode)
                {
                    ProcessHelper.ShutDownAllManagerIntegrationConsoleHostProcesses();

                    StartManagerIntegrationConsoleHostProcess =
                        ProcessHelper.StartManagerIntegrationConsoleHostProcess(NumberOfNodesToStart);
                }
                else
                {
                    var task = AppDomainHelper.CreateAppDomainForManagerIntegrationConsoleHost(_buildMode);

                    task.Start();
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            ManagerApiHelper.CheckJobHistoryStatusTimer.Stop();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            ManagerApiHelper.CheckJobHistoryStatusTimer.Stop();

            if (AppDomainHelper.AppDomains.Any())
            {
                foreach (var appDomain in AppDomainHelper.AppDomains.Values)
                {
                    AppDomain.Unload(appDomain);
                }
            }

            ProcessHelper.CloseProcess(StartManagerIntegrationConsoleHostProcess);
        }

        private const int NumberOfNodesToStart = 1;

        private bool _startUpManagerAndNodeManually;

        private bool _clearDatabase;

        private bool _debugMode = true;

        private string _buildMode = "Debug";

        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (IntegrationTestsOneManagerAndManyNodes));

        private Process StartManagerIntegrationConsoleHostProcess { get; set; }

        private ManagerApiHelper ManagerApiHelper { get; set; }

        [Test]
        public void Create5RequestShouldReturnBothCancelAndDeleteStatuses()
        {
            JobHelper.GiveNodesTimeToInitialize();

            List<JobRequestModel> requests = JobHelper.GenerateLongRunningParamsRequests(5);

            var timeout = JobHelper.GenerateTimeoutTimeInMinutes(requests.Count);

            List<Task> tasks = new List<Task>();

            foreach (var jobRequestModel in requests)
            {
                tasks.Add(ManagerApiHelper.CreateManagerDoThisTask(jobRequestModel));
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

                cancelJobTask.Start();
            };

            ManagerApiHelper.CheckJobHistoryStatusTimer.Start();

            Parallel.ForEach(tasks,
                             task => { task.Start(); });

            ManagerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait(TimeSpan.FromMinutes(1));

            ManagerApiHelper.CheckJobHistoryStatusTimer.Stop();

            Assert.IsTrue(ManagerApiHelper.CheckJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.CanceledStatus ||
                                                                                        pair.Value == StatusConstants.DeletedStatus));
        }

        [Test]
        public void JobShouldHaveStatusFailedIfFailed()
        {
            Logger.Info("Starting test : JobShouldHaveStatusFailedIfFailed");

            JobHelper.GiveNodesTimeToInitialize();

            List<JobRequestModel> requests = JobHelper.GenerateFailingJobParamsRequests(1);

            var timeout = JobHelper.GenerateTimeoutTimeInMinutes(requests.Count);

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

            ManagerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

            ManagerApiHelper.CheckJobHistoryStatusTimer.Stop();

            Assert.IsTrue(ManagerApiHelper.CheckJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.FailedStatus));
        }

        [Test]
        public void CancelWrongJobs()
        {
            Logger.Info("Starting test : CancelWrongJobs");

            JobHelper.GiveNodesTimeToInitialize();

            List<JobRequestModel> requests = JobHelper.GenerateLongRunningParamsRequests(1);

            var timeout = JobHelper.GenerateTimeoutTimeInMinutes(requests.Count,
                                                                 5);
            List<Task> tasks = new List<Task>();

            foreach (var jobRequestModel in requests)
            {
                tasks.Add(ManagerApiHelper.CreateManagerDoThisTask(jobRequestModel));

                Logger.Debug("Created task for add job :" + jobRequestModel.Name);
            }

            ManagerApiHelper.CheckJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(requests.Count,
                                                                                         5000,
                                                                                         StatusConstants.SuccessStatus,
                                                                                         StatusConstants.DeletedStatus,
                                                                                         StatusConstants.FailedStatus,
                                                                                         StatusConstants.CanceledStatus);

            ManagerApiHelper.CheckJobHistoryStatusTimer.GuidAddedEventHandler += (sender,
                                                                                  args) =>
            {
                var newGuid = Guid.NewGuid();

                var cancelJobTask = ManagerApiHelper.CreateManagerCancelTask(newGuid);

                Logger.Debug("CancelWrongJobs : Created task for cancel job :" + newGuid);

                cancelJobTask.Start();
            };

            ManagerApiHelper.CheckJobHistoryStatusTimer.Start();

            Parallel.ForEach(tasks,
                             task => { task.Start(); });

            ManagerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

            ManagerApiHelper.CheckJobHistoryStatusTimer.Stop();

            Assert.IsTrue(ManagerApiHelper.CheckJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.SuccessStatus));
        }

        [Test]
        public void ShouldBeAbleToCreate5SuccessJobRequest()
        {
            LogHelper.LogInfoWithLineNumber("starting test...");

            JobHelper.GiveNodesTimeToInitialize();

            List<JobRequestModel> requests = JobHelper.GenerateTestJobParamsRequests(5);

            TimeSpan timeout = JobHelper.GenerateTimeoutTimeInMinutes(requests.Count);

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

            ManagerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait();

            ManagerApiHelper.CheckJobHistoryStatusTimer.Stop();

            Assert.IsTrue(ManagerApiHelper.CheckJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.SuccessStatus));

            LogHelper.LogInfoWithLineNumber("finished test.");
        }
    }
}