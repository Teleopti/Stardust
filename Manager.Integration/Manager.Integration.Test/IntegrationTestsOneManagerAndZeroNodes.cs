using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
    public class IntegrationTestsOneManagerAndZeroNodes
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (IntegrationTestsOneManagerAndZeroNodes));

        private bool _clearDatabase = true;
        private string _buildMode = "Debug";


        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));


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

            JobHelper.GiveNodesTimeToInitialize(60);

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

        private const int NumberOfNodesToStart = 0;

        [Test]
        public void JobShouldJustBeQueuedIfNoNodes()
        {
            LogHelper.LogInfoWithLineNumber("Start test.",
                                            Logger);

            List<JobRequestModel> requests = JobHelper.GenerateTestJobParamsRequests(1);

            var timeout = JobHelper.GenerateTimeoutTimeInSeconds(requests.Count,
                                                                 30);

            var managerApiHelper = new ManagerApiHelper(new CheckJobHistoryStatusTimer(requests.Count,
                                                                                       StatusConstants.SuccessStatus,
                                                                                       StatusConstants.DeletedStatus,
                                                                                       StatusConstants.FailedStatus,
                                                                                       StatusConstants.CanceledStatus));

            List<Task> tasks = new List<Task>();

            foreach (var jobRequestModel in requests)
            {
                tasks.Add(managerApiHelper.CreateManagerDoThisTask(jobRequestModel));
            }

            Parallel.ForEach(tasks,
                             task => { task.Start(); });

            managerApiHelper.CheckJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

            Assert.IsTrue(managerApiHelper.CheckJobHistoryStatusTimer.Guids.Count > 0);

            managerApiHelper.Dispose();
        }
    }
}