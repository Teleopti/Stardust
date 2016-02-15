using System;
using System.Collections.Generic;
using System.Configuration;
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
using Manager.Integration.Test.Tasks;
using Manager.Integration.Test.Timers;
using NUnit.Framework;

namespace Manager.Integration.Test
{
    [TestFixture]
    public class IntegrationTestsOneManagerAndManyNodes
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (IntegrationTestsOneManagerAndManyNodes));


        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

            LogHelper.LogInfoWithLineNumber("Start TestFixtureSetUp",
                                            Logger);

            TryCreateSqlLoggingTable();


#if (DEBUG)
            // Do nothing.
#else
            _clearDatabase = true;
            _buildMode = "Release";
#endif

            if (_clearDatabase)
            {
                DatabaseHelper.TryClearDatabase();
            }

            CancellationTokenSource = new CancellationTokenSource();

            AppDomainTask = new AppDomainTask(_buildMode);

            Task = AppDomainTask.StartTask(CancellationTokenSource,
                                           NumberOfNodesToStart);

            JobHelper.GiveNodesTimeToInitialize();

            LogHelper.LogInfoWithLineNumber("Finshed TestFixtureSetUp",
                                            Logger);
        }

        private Task Task { get; set; }

        private AppDomainTask AppDomainTask { get; set; }

        private CancellationTokenSource CancellationTokenSource { get; set; }

        private void CurrentDomain_UnhandledException(object sender,
                                                      UnhandledExceptionEventArgs e)
        {
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
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.LogInfoWithLineNumber("Start TestFixtureTearDown",
                                            Logger);

            if (AppDomainTask != null)
            {
                AppDomainTask.Dispose();
            }            

            LogHelper.LogInfoWithLineNumber("Finished TestFixtureTearDown",
                                            Logger);
        }

        private const int NumberOfNodesToStart = 1;

        private bool _clearDatabase = true;

        private string _buildMode = "Debug";

        [Test]
        public void CreateSeveralRequestShouldReturnBothCancelAndDeleteStatuses()
        {
            LogHelper.LogInfoWithLineNumber("Start test.",
                                            Logger);

            List<JobRequestModel> requests = JobHelper.GenerateLongRunningParamsRequests(2*NumberOfNodesToStart);

            var timeout = JobHelper.GenerateTimeoutTimeInMinutes(requests.Count,
                                                                 5);

            List<Task> tasks = new List<Task>();

            var managerApiHelper = new ManagerApiHelper(new CheckJobHistoryStatusTimer(requests.Count,
                                                                                       StatusConstants.SuccessStatus,
                                                                                       StatusConstants.DeletedStatus,
                                                                                       StatusConstants.FailedStatus,
                                                                                       StatusConstants.CanceledStatus));

            foreach (var jobRequestModel in requests)
            {
                tasks.Add(managerApiHelper.CreateManagerDoThisTask(jobRequestModel));
            }

            managerApiHelper.CheckJobHistoryStatusTimer.GuidAddedEventHandler += (sender,
                                                                                  args) =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));

                var cancelJobTask = managerApiHelper.CreateManagerCancelTask(args.Guid);

                cancelJobTask.Start();
            };

            Parallel.ForEach(tasks,
                             task => { task.Start(); });

            managerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

            Assert.IsTrue(managerApiHelper.CheckJobHistoryStatusTimer.Guids.Count > 0);
            Assert.IsTrue(managerApiHelper.CheckJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.CanceledStatus ||
                                                                                        pair.Value == StatusConstants.DeletedStatus));
            managerApiHelper.Dispose();

            LogHelper.LogInfoWithLineNumber("Finished test.",
                                            Logger);
        }

        [Test]
        public void JobShouldHaveStatusFailedIfFailed()
        {
            LogHelper.LogInfoWithLineNumber("Starting test.",
                                            Logger);

            List<JobRequestModel> requests = JobHelper.GenerateFailingJobParamsRequests(1);

            var timeout = JobHelper.GenerateTimeoutTimeInMinutes(requests.Count,
                                                                 5);

            List<Task> tasks = new List<Task>();

            var managerApiHelper = new ManagerApiHelper(new CheckJobHistoryStatusTimer(requests.Count,
                                                                                       StatusConstants.SuccessStatus,
                                                                                       StatusConstants.DeletedStatus,
                                                                                       StatusConstants.FailedStatus,
                                                                                       StatusConstants.CanceledStatus));
            foreach (var jobRequestModel in requests)
            {
                tasks.Add(managerApiHelper.CreateManagerDoThisTask(jobRequestModel));
            }

            Parallel.ForEach(tasks,
                             task => { task.Start(); });

            managerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

            Assert.IsTrue(managerApiHelper.CheckJobHistoryStatusTimer.Guids.Count > 0);
            Assert.IsTrue(managerApiHelper.CheckJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.FailedStatus));

            managerApiHelper.Dispose();

            LogHelper.LogInfoWithLineNumber("Finished test.",
                                            Logger);
        }

        [Test]
        public void CancelWrongJobs()
        {
            LogHelper.LogInfoWithLineNumber("Starting test.",
                                            Logger);

            List<JobRequestModel> requests = JobHelper.GenerateTestJobParamsRequests(1);

            var timeout = JobHelper.GenerateTimeoutTimeInMinutes(requests.Count,
                                                                 5);
            List<Task> tasks = new List<Task>();

            var managerApiHelper = new ManagerApiHelper(new CheckJobHistoryStatusTimer(requests.Count,
                                                                                       StatusConstants.SuccessStatus,
                                                                                       StatusConstants.DeletedStatus,
                                                                                       StatusConstants.FailedStatus,
                                                                                       StatusConstants.CanceledStatus));

            foreach (var jobRequestModel in requests)
            {
                tasks.Add(managerApiHelper.CreateManagerDoThisTask(jobRequestModel));

                LogHelper.LogDebugWithLineNumber("Created task for add job :" + jobRequestModel.Name,
                                                 Logger);
            }


            managerApiHelper.CheckJobHistoryStatusTimer.GuidAddedEventHandler += (sender,
                                                                                  args) =>
            {
                var newGuid = Guid.NewGuid();

                var cancelJobTask = managerApiHelper.CreateManagerCancelTask(newGuid);

                LogHelper.LogDebugWithLineNumber("CancelWrongJobs : Created task for cancel job :" + newGuid,
                                                 Logger);

                cancelJobTask.Start();
            };

            Parallel.ForEach(tasks,
                             task => { task.Start(); });

            managerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

            Assert.IsTrue(managerApiHelper.CheckJobHistoryStatusTimer.Guids.Count > 0);
            Assert.IsTrue(managerApiHelper.CheckJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.SuccessStatus));

            managerApiHelper.Dispose();

            LogHelper.LogInfoWithLineNumber("Finished test.",
                                            Logger);
        }

        [Test]
        public void ShouldBeAbleToCreateManySuccessJobRequest()
        {
            LogHelper.LogInfoWithLineNumber("Start test.",
                                            Logger);

            List<JobRequestModel> requests = JobHelper.GenerateTestJobParamsRequests(NumberOfNodesToStart*1);

            TimeSpan timeout = JobHelper.GenerateTimeoutTimeInMinutes(requests.Count,
                                                                      5);

            List<Task> tasks = new List<Task>();

            var managerApiHelper = new ManagerApiHelper(new CheckJobHistoryStatusTimer(requests.Count,
                                                                                       StatusConstants.SuccessStatus,
                                                                                       StatusConstants.DeletedStatus,
                                                                                       StatusConstants.FailedStatus,
                                                                                       StatusConstants.CanceledStatus));

            foreach (var jobRequestModel in requests)
            {
                tasks.Add(managerApiHelper.CreateManagerDoThisTask(jobRequestModel));
            }

            Parallel.ForEach(tasks,
                             task => { task.Start(); });

            managerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

            bool condition =
                managerApiHelper.CheckJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.SuccessStatus);

            Assert.IsTrue(condition);
            Assert.IsTrue(managerApiHelper.CheckJobHistoryStatusTimer.Guids.Count > 0);

            managerApiHelper.Dispose();

            LogHelper.LogInfoWithLineNumber("Finished test.",
                                            Logger);
        }
    }
}