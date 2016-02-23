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
using Manager.Integration.Test.Notifications;
using Manager.Integration.Test.Properties;
using Manager.Integration.Test.Scripts;
using Manager.Integration.Test.Tasks;
using Manager.Integration.Test.Timers;
using Manager.Integration.Test.Validators;
using NUnit.Framework;

namespace Manager.Integration.Test
{
    [TestFixture]
    public class OneManagerAndFiveNodesLoadTests
    {
		[TearDown]
		public void TearDown()
		{
		}

        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (OneManagerAndFiveNodesLoadTests));

        private string ManagerDbConnectionString { get; set; }

		private bool _clearDatabase = true;

		private string _buildMode = "Debug";

		[TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ManagerDbConnectionString =
                ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;

            var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

            LogHelper.LogDebugWithLineNumber("Start TestFixtureSetUp",
                                            Logger);

            TryCreateSqlLoggingTable(ManagerDbConnectionString);


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

			Task = AppDomainTask.StartTask(CancellationTokenSource, 5);

            LogHelper.LogDebugWithLineNumber("Finished TestFixtureSetUp",
                                            Logger);
        }

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			LogHelper.LogDebugWithLineNumber("Start TestFixtureTearDown",
											Logger);

			if (AppDomainTask != null)
			{
				AppDomainTask.Dispose();
			}

			LogHelper.LogDebugWithLineNumber("Finished TestFixtureTearDown",
											Logger);
		}

		private Task Task { get; set; }

        private AppDomainTask AppDomainTask { get; set; }

        private CancellationTokenSource CancellationTokenSource { get; set; }

        private void CurrentDomain_UnhandledException(object sender,
                                                      UnhandledExceptionEventArgs e)
        {
			var exp = e.ExceptionObject as Exception;

			if (exp != null)
			{
				LogHelper.LogFatalWithLineNumber(exp.Message,
												 Logger,
												 exp);
			}
		}

		private static void TryCreateSqlLoggingTable(string connectionString)
        {
            LogHelper.LogDebugWithLineNumber("Run sql script to create logging file started.",
                                            Logger);

			var scriptFile =
                new FileInfo(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                          Settings.Default.CreateLoggingTableSqlScriptLocationAndFileName));

            ScriptExecuteHelper.ExecuteScriptFile(scriptFile,
                                                  connectionString);

            LogHelper.LogDebugWithLineNumber("Run sql script to create logging file finished.",
                                            Logger);
        }

        /// <summary>
        ///     DO NOT FORGET TO RUN COMMAND BELOW AS ADMINISTRATOR.
        ///     netsh http add urlacl url=http://+:9050/ user=everyone listen=yes
        /// </summary>
        [Test]
        public void ShouldBeAbleToCreateManySuccessJobRequestTest()
        {
            LogHelper.LogDebugWithLineNumber("Start.",
                                            Logger);

			var createNewJobRequests = JobHelper.GenerateTestJobParamsRequests(50);

            var checkJobHistoryStatusTimer = new CheckJobHistoryStatusTimer(createNewJobRequests.Count,
                                                                            StatusConstants.SuccessStatus,
                                                                            StatusConstants.DeletedStatus,
                                                                            StatusConstants.FailedStatus,
                                                                            StatusConstants.CanceledStatus);

            //---------------------------------------------
            // Create timeout time.
            //---------------------------------------------
			var timeout =
                JobHelper.GenerateTimeoutTimeInMinutes(createNewJobRequests.Count);

            //---------------------------------------------
            // Create jobs.
            //---------------------------------------------
			var jobManagerTaskCreators = new List<JobManagerTaskCreator>();

            foreach (var jobRequestModel in createNewJobRequests)
            {
                var jobManagerTaskCreator =
                    new JobManagerTaskCreator(checkJobHistoryStatusTimer);

                jobManagerTaskCreator.CreateNewJobToManagerTask(jobRequestModel);

                jobManagerTaskCreators.Add(jobManagerTaskCreator);
            }

            //---------------------------------------------
            // Notify when all 5 nodes are up. 
            //---------------------------------------------
			var sqlNotiferCancellationTokenSource = new CancellationTokenSource();

			var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

            Task task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(5,
                                                                   sqlNotiferCancellationTokenSource,
																   IntegerValidators.Value1IsEqualToValue2Validator);
            task.Start();

            LogHelper.LogDebugWithLineNumber("Waiting for all 5 nodes to start up.",
                                            Logger);

            sqlNotifier.NotifyWhenAllNodesAreUp.Wait(timeout);

            sqlNotifier.Dispose();

            LogHelper.LogDebugWithLineNumber("All 5 nodes have started.",
                                            Logger);

            //---------------------------------------------
            // Execute all jobs. 
            //---------------------------------------------
			var startJobTaskHelper = new StartJobTaskHelper();

			var taskHelper = startJobTaskHelper.ExecuteCreateNewJobTasks(jobManagerTaskCreators,
                                                                          CancellationTokenSource,
                                                                          TimeSpan.FromMilliseconds(200));

            //---------------------------------------------
            // Wait for all jobs to finish.
            //---------------------------------------------
            checkJobHistoryStatusTimer.ManualResetEventSlim.Wait(timeout);

            //---------------------------------------------
            // Assert.
            //---------------------------------------------
            Assert.IsTrue(checkJobHistoryStatusTimer.Guids.Count == createNewJobRequests.Count);
            Assert.IsTrue(checkJobHistoryStatusTimer.Guids.All(pair => pair.Value == StatusConstants.SuccessStatus));

            //---------------------------------------------
            // Cancel tasks.
            //---------------------------------------------
            CancellationTokenSource.Cancel();

            //---------------------------------------------
            // Dispose.
            //---------------------------------------------
            foreach (var jobManagerTaskCreator in jobManagerTaskCreators)
            {
                jobManagerTaskCreator.Dispose();
            }

            taskHelper.Dispose();

            //---------------------------------------------
            // Log.
            //---------------------------------------------
            LogHelper.LogDebugWithLineNumber("Finished.",
                                            Logger);
        }
    }
}