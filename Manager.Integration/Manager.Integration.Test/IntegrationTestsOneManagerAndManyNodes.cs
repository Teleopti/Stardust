using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Manager.Integration.Test.Constants;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Properties;
using Manager.Integration.Test.Scripts;
using Manager.Integration.Test.Timers;
using NUnit.Framework;

namespace Manager.Integration.Test
{
    [TestFixture]
    public class IntegrationTestsOneManagerAndManyNodes
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (IntegrationTestsOneManagerAndManyNodes));

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

            TryCreateSqlLoggingTable();

#if (DEBUG)
            // Do nothing.
#else
            _clearDatabase = true;
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
                    var task = AppDomainHelper.CreateAppDomainForManagerIntegrationConsoleHost(_buildMode,
                                                                                               NumberOfNodesToStart);

                    task.Start();
                }
            }
        }

        private static void TryCreateSqlLoggingTable()
        {
            LogHelper.LogInfoWithLineNumber("Run sql script to create logging file started.",
                                            Logger);

            FileInfo scriptFile =
                new FileInfo(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                          Settings.Default.CreateLoggingTableSqlScriptLocationAndFileName));

            ScriptExecuteHelper.ExecuteScriptFile(scriptFile,
                                                  ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString);

            LogHelper.LogInfoWithLineNumber("Run sql script to create logging file finished.",
                                            Logger);
        }

        [TearDown]
        public void TearDown()
        {
            if (ManagerApiHelper != null &&
                ManagerApiHelper.CheckJobHistoryStatusTimer != null)
            {
                ManagerApiHelper.CheckJobHistoryStatusTimer.Stop();
            }
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            if (ManagerApiHelper != null &&
                ManagerApiHelper.CheckJobHistoryStatusTimer != null)
            {
                ManagerApiHelper.CheckJobHistoryStatusTimer.Stop();
            }

            if (AppDomainHelper.AppDomains != null &&
                AppDomainHelper.AppDomains.Any())
            {
                foreach (var appDomain in AppDomainHelper.AppDomains.Values)
                {
                    AppDomain.Unload(appDomain);
                }
            }

            ProcessHelper.CloseProcess(StartManagerIntegrationConsoleHostProcess);
        }

        private const int NumberOfNodesToStart = 1;

        private bool _startUpManagerAndNodeManually = false;

        private bool _clearDatabase = true;

        private bool _debugMode = true;

        private string _buildMode = "Debug";

        private Process StartManagerIntegrationConsoleHostProcess { get; set; }

        private ManagerApiHelper ManagerApiHelper { get; set; }

        [Test]
        public void Create5RequestShouldReturnBothCancelAndDeleteStatuses()
        {
            LogHelper.LogInfoWithLineNumber("Start test.",
                                            Logger);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

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
                                                                                         cancellationTokenSource,
                                                                                         StatusConstants.CanceledStatus,
                                                                                         StatusConstants.DeletedStatus,
                                                                                         StatusConstants.SuccessStatus,
                                                                                         StatusConstants.FailedStatus);

            ManagerApiHelper.CheckJobHistoryStatusTimer.GuidAddedEventHandler += (sender,
                                                                                  args) =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));

                var cancelJobTask = ManagerApiHelper.CreateManagerCancelTask(args.Guid);

                cancelJobTask.Start();
            };

            ManagerApiHelper.CheckJobHistoryStatusTimer.Start();

            Parallel.ForEach(tasks,
                             task => { task.Start(); });

            ManagerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait(TimeSpan.FromMinutes(1));

            ManagerApiHelper.CheckJobHistoryStatusTimer.Stop();
            ManagerApiHelper.CheckJobHistoryStatusTimer.CancelAllRequest();

            Assert.IsTrue(ManagerApiHelper.CheckJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.CanceledStatus ||
                                                                                        pair.Value == StatusConstants.DeletedStatus));

            LogHelper.LogInfoWithLineNumber("Finished test.",
                                            Logger);
        }

        [Test]
        public void JobShouldHaveStatusFailedIfFailed()
        {
            LogHelper.LogInfoWithLineNumber("Starting test.",
                                            Logger);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

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
                                                                                         cancellationTokenSource,
                                                                                         StatusConstants.SuccessStatus,
                                                                                         StatusConstants.DeletedStatus,
                                                                                         StatusConstants.FailedStatus,
                                                                                         StatusConstants.CanceledStatus);
            ManagerApiHelper.CheckJobHistoryStatusTimer.Start();

            Parallel.ForEach(tasks,
                             task => { task.Start(); });

            ManagerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

            ManagerApiHelper.CheckJobHistoryStatusTimer.Stop();
            ManagerApiHelper.CheckJobHistoryStatusTimer.CancelAllRequest();

            Assert.IsTrue(ManagerApiHelper.CheckJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.FailedStatus));

            LogHelper.LogInfoWithLineNumber("Finished test.",
                                            Logger);
        }

        [Test]
        public void CancelWrongJobs()
        {
            LogHelper.LogInfoWithLineNumber("Starting test.",
                                            Logger);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            JobHelper.GiveNodesTimeToInitialize();

            List<JobRequestModel> requests = JobHelper.GenerateLongRunningParamsRequests(1);

            var timeout = JobHelper.GenerateTimeoutTimeInMinutes(requests.Count,
                                                                 5);
            List<Task> tasks = new List<Task>();

            foreach (var jobRequestModel in requests)
            {
                tasks.Add(ManagerApiHelper.CreateManagerDoThisTask(jobRequestModel));

                LogHelper.LogDebugWithLineNumber("Created task for add job :" + jobRequestModel.Name,
                                                 Logger);
            }

            ManagerApiHelper.CheckJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(requests.Count,
                                                                                         5000,
                                                                                         cancellationTokenSource,
                                                                                         StatusConstants.SuccessStatus,
                                                                                         StatusConstants.DeletedStatus,
                                                                                         StatusConstants.FailedStatus,
                                                                                         StatusConstants.CanceledStatus);

            ManagerApiHelper.CheckJobHistoryStatusTimer.GuidAddedEventHandler += (sender,
                                                                                  args) =>
            {
                var newGuid = Guid.NewGuid();

                var cancelJobTask = ManagerApiHelper.CreateManagerCancelTask(newGuid);

                LogHelper.LogDebugWithLineNumber("CancelWrongJobs : Created task for cancel job :" + newGuid,
                                                 Logger);

                cancelJobTask.Start();
            };

            ManagerApiHelper.CheckJobHistoryStatusTimer.Start();

            Parallel.ForEach(tasks,
                             task => { task.Start(); });

            ManagerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

            ManagerApiHelper.CheckJobHistoryStatusTimer.Stop();
            ManagerApiHelper.CheckJobHistoryStatusTimer.CancelAllRequest();

            Assert.IsTrue(ManagerApiHelper.CheckJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.SuccessStatus));

            LogHelper.LogInfoWithLineNumber("Finished test.",
                                            Logger);
        }

        [Test]
        public void ShouldBeAbleToCreateManySuccessJobRequest()
        {
            LogHelper.LogInfoWithLineNumber("Start test.",
                                            Logger);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            JobHelper.GiveNodesTimeToInitialize();

            List<JobRequestModel> requests = JobHelper.GenerateTestJobParamsRequests(NumberOfNodesToStart * 2);

            TimeSpan timeout = JobHelper.GenerateTimeoutTimeInMinutes(requests.Count);

            List<Task> tasks = new List<Task>();

            foreach (var jobRequestModel in requests)
            {
                tasks.Add(ManagerApiHelper.CreateManagerDoThisTask(jobRequestModel));
            }

            ManagerApiHelper.CheckJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(requests.Count,
                                                                                         5000,
                                                                                         cancellationTokenSource,
                                                                                         StatusConstants.SuccessStatus,
                                                                                         StatusConstants.DeletedStatus,
                                                                                         StatusConstants.FailedStatus,
                                                                                         StatusConstants.CanceledStatus);

            ManagerApiHelper.CheckJobHistoryStatusTimer.Start();

            Parallel.ForEach(tasks,
                             task => { task.Start(); });

            ManagerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait();

            ManagerApiHelper.CheckJobHistoryStatusTimer.CancelAllRequest();
            ManagerApiHelper.CheckJobHistoryStatusTimer.Stop();

            bool condition =
                ManagerApiHelper.CheckJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.SuccessStatus);

            Assert.IsTrue(condition);

            LogHelper.LogInfoWithLineNumber("Finished test.",
                                            Logger);
        }
    }
}