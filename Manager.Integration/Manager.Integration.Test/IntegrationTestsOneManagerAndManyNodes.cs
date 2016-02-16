using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
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

            LogHelper.LogInfoWithLineNumber("JobHelper.GiveNodesTimeToInitialize",
                                            Logger);

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

        [Test, Ignore]
        public void CancelWrongJobsTest()
        {
            LogHelper.LogInfoWithLineNumber("Start.",
                                            Logger);

            List<JobRequestModel> createNewJobRequests = JobHelper.GenerateTestJobParamsRequests(1);

            LogHelper.LogInfoWithLineNumber(createNewJobRequests.Count + " jobs will be created.",
                                            Logger);

            TimeSpan timeout = JobHelper.GenerateTimeoutTimeInMinutes(createNewJobRequests.Count,
                                                                      5);

            List<JobManagerTaskCreator> jobManagerTaskCreators = new List<JobManagerTaskCreator>();

            var checkJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(createNewJobRequests.Count,
                                                                            StatusConstants.SuccessStatus,
                                                                            StatusConstants.DeletedStatus,
                                                                            StatusConstants.FailedStatus,
                                                                            StatusConstants.CanceledStatus);

            foreach (var jobRequestModel in createNewJobRequests)
            {
                var jobManagerTaskCreator = new JobManagerTaskCreator(checkJobHistoryStatusTimer);

                jobManagerTaskCreator.CreateNewJobToManagerTask(jobRequestModel);

                jobManagerTaskCreators.Add(jobManagerTaskCreator);
            }

            checkJobHistoryStatusTimer.GuidAddedEventHandler += (sender, args) =>
            {
                var newGuid = Guid.NewGuid();

                var jobManagerTaskCreator = new JobManagerTaskCreator(checkJobHistoryStatusTimer);

                jobManagerTaskCreator.CreateDeleteJobToManagerTask(newGuid);

                jobManagerTaskCreator.StartDeleteJobToManagerTask(timeout);

                jobManagerTaskCreator.Dispose();
            };

            StartJobTaskHelper startJobTaskHelper = new StartJobTaskHelper();

            var taskHlp = startJobTaskHelper.ExecuteTasks(jobManagerTaskCreators,
                                                          CancellationTokenSource,
                                                          timeout);

            checkJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

            Assert.IsTrue(checkJobHistoryStatusTimer.Guids.Count == createNewJobRequests.Count);
            Assert.IsTrue(checkJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.SuccessStatus));

            taskHlp.Dispose();

            foreach (var jobManagerTaskCreator in jobManagerTaskCreators)
            {
                jobManagerTaskCreator.Dispose();
            }

            LogHelper.LogInfoWithLineNumber("Finished.",
                                            Logger);

        }

        [Test]
        public void ShouldBeAbleToCreateManySuccessJobRequestTest()
        {
            LogHelper.LogInfoWithLineNumber("Start.",
                                            Logger);

            List<JobRequestModel> createNewJobRequests =
                JobHelper.GenerateTestJobParamsRequests(NumberOfNodesToStart * 1);

            LogHelper.LogInfoWithLineNumber(createNewJobRequests.Count + " jobs will be created." ,
                                            Logger);


            TimeSpan timeout = JobHelper.GenerateTimeoutTimeInMinutes(createNewJobRequests.Count,
                                                                      5);

            List<JobManagerTaskCreator> jobManagerTaskCreators = new List<JobManagerTaskCreator>();

            var checkJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(createNewJobRequests.Count,
                                                                            StatusConstants.SuccessStatus,
                                                                            StatusConstants.DeletedStatus,
                                                                            StatusConstants.FailedStatus,
                                                                            StatusConstants.CanceledStatus);

            foreach (var jobRequestModel in createNewJobRequests)
            {
                var jobManagerTaskCreator = new JobManagerTaskCreator(checkJobHistoryStatusTimer);

                jobManagerTaskCreator.CreateNewJobToManagerTask(jobRequestModel);

                jobManagerTaskCreators.Add(jobManagerTaskCreator);
            }

            StartJobTaskHelper startJobTaskHelper = new StartJobTaskHelper();

            var taskHlp = startJobTaskHelper.ExecuteTasks(jobManagerTaskCreators,
                                                          CancellationTokenSource,
                                                          timeout);

            checkJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

            Assert.IsTrue(checkJobHistoryStatusTimer.Guids.Count == createNewJobRequests.Count);
            Assert.IsTrue(checkJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.SuccessStatus));

            taskHlp.Dispose();

            foreach (var jobManagerTaskCreator in jobManagerTaskCreators)
            {
                jobManagerTaskCreator.Dispose();
            }

            LogHelper.LogInfoWithLineNumber("Finished.",
                                            Logger);
        }
    }
}