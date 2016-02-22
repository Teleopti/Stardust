using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Notifications;
using Manager.Integration.Test.Properties;
using Manager.Integration.Test.Scripts;
using Manager.Integration.Test.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Manager.Integration.Test
{
    [TestFixture]
    public class ManagerIntegrationTestControllerTests
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (ManagerIntegrationTestControllerTests));

        private bool _clearDatabase = true;
        private string _buildMode = "Debug";

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

        private static void TryCreateSqlLoggingTable(string connectionString)
        {
            LogHelper.LogInfoWithLineNumber("Run sql script to create logging file started.",
                                            Logger);

            FileInfo scriptFile =
                new FileInfo(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                          Settings.Default.CreateLoggingTableSqlScriptLocationAndFileName));

            ScriptExecuteHelper.ExecuteScriptFile(scriptFile,
                                                  connectionString);

            LogHelper.LogInfoWithLineNumber("Run sql script to create logging file finished.",
                                            Logger);
        }


        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
#if (DEBUG)
            // Do nothing.
#else
            _clearDatabase = true;
            _buildMode = "Debug";
#endif
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ManagerDbConnectionString =
                ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;

            var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

            LogHelper.LogInfoWithLineNumber("Start TestFixtureSetUp",
                                            Logger);

            TryCreateSqlLoggingTable(ManagerDbConnectionString);

            if (_clearDatabase)
            {
                DatabaseHelper.TryClearDatabase();
            }

            CancellationTokenSource = new CancellationTokenSource();

            AppDomainTask = new AppDomainTask(_buildMode);

            Task = AppDomainTask.StartTask(cancellationTokenSource: CancellationTokenSource,
                                           numberOfNodes: 5);

            LogHelper.LogInfoWithLineNumber("Finshed TestFixtureSetUp",
                                            Logger);
        }

        private void CurrentDomain_UnhandledException(object sender,
                                                      UnhandledExceptionEventArgs e)
        {
        }

        private string ManagerDbConnectionString { get; set; }

        private Task Task { get; set; }

        private AppDomainTask AppDomainTask { get; set; }

        private CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        ///     DO NOT FORGET TO RUN COMMAND BELOW AS ADMINISTRATOR.
        ///     netsh http add urlacl url=http://+:9100/ user=everyone listen=yes
        /// </summary>
        [Test]
        public async void ShouldUnloadNode1AppDomain()
        {
            LogHelper.LogInfoWithLineNumber("Start test.",
                                            Logger);


            //---------------------------------------------
            // Notify when all 5 nodes are up and running. 
            //---------------------------------------------
            CancellationTokenSource sqlNotiferCancellationTokenSource = new CancellationTokenSource();

            SqlNotifier sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

            Task task = sqlNotifier.CreateNotifyWhenAllNodesAreUpTask(5,
                                                                      sqlNotiferCancellationTokenSource);
            task.Start();

            LogHelper.LogInfoWithLineNumber("Waiting for all 5 nodes to start up.",
                                            Logger);

            sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(10));

            sqlNotifier.Dispose();

            LogHelper.LogInfoWithLineNumber("All 5 nodes has started.",
                                            Logger);

            //---------------------------------------------
            // Start actual test.
            //---------------------------------------------
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            HttpResponseMessage response = null;

            using (var client = new HttpClient())
            {
                UriBuilder uriBuilder =
                    new UriBuilder(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

                uriBuilder.Path += "appdomain/" + "Node1.config";

                Uri uri = uriBuilder.Uri;

                LogHelper.LogInfoWithLineNumber("Start calling Delete Async ( " + uri + " ) ",
                                                Logger);

                try
                {
                    response = await client.DeleteAsync(uriBuilder.Uri,
                                                        cancellationTokenSource.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        LogHelper.LogInfoWithLineNumber("Succeded calling Delete Async ( " + uri + " ) ",
                                                        Logger);
                    }
                }
                catch (Exception exp)
                {
                    LogHelper.LogErrorWithLineNumber(exp.Message,
                                                     Logger,
                                                     exp);
                }
            }

            Assert.IsTrue(response.IsSuccessStatusCode);

            cancellationTokenSource.Cancel();

            LogHelper.LogInfoWithLineNumber("Finished test.",
                                            Logger);
        }

        /// <summary>
        ///     DO NOT FORGET TO RUN COMMAND BELOW AS ADMINISTRATOR.
        ///     netsh http add urlacl url=http://+:9100/ user=everyone listen=yes
        /// </summary>
        [Test]
        public async void ShouldReturnAllAppDomainKeys()
        {
            LogHelper.LogInfoWithLineNumber("Start test.",
                                            Logger);

            //---------------------------------------------
            // Notify when all 5 nodes are up. 
            //---------------------------------------------
            CancellationTokenSource sqlNotiferCancellationTokenSource = new CancellationTokenSource();

            SqlNotifier sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

            Task task = sqlNotifier.CreateNotifyWhenAllNodesAreUpTask(5,
                                                                      sqlNotiferCancellationTokenSource);
            task.Start();

            LogHelper.LogInfoWithLineNumber("Waiting for all 5 nodes to start up.",
                                            Logger);

            sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(10));

            sqlNotifier.Dispose();

            LogHelper.LogInfoWithLineNumber("All 5 nodes has started.",
                                            Logger);

            //---------------------------------------------
            // Start actual test.
            //---------------------------------------------
            HttpResponseMessage response = null;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                UriBuilder uriBuilder =
                    new UriBuilder(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

                uriBuilder.Path += "appdomain";

                Uri uri = uriBuilder.Uri;

                LogHelper.LogInfoWithLineNumber("Start calling Get Async ( " + uri + " ) ",
                                                Logger);


                try
                {
                    response = await client.GetAsync(uriBuilder.Uri,
                                                     cancellationTokenSource.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        LogHelper.LogInfoWithLineNumber("Succeeded calling Get Async ( " + uri + " ) ",
                                                        Logger);

                        string content = await response.Content.ReadAsStringAsync();

                        List<string> list =
                            JsonConvert.DeserializeObject<List<string>>(content);

                        if (list.Any())
                        {
                            foreach (var l in list)
                            {
                                LogHelper.LogInfoWithLineNumber(l,
                                                                Logger);
                            }
                        }

                        Assert.IsTrue(list.Any());
                    }
                }

                catch (Exception exp)
                {
                    LogHelper.LogErrorWithLineNumber(exp.Message,
                                                     Logger,
                                                     exp);
                }

            }

            Assert.IsTrue(response.IsSuccessStatusCode);

            cancellationTokenSource.Cancel();

            task.Dispose();

            LogHelper.LogInfoWithLineNumber("Finished test.",
                                            Logger);
        }
    }
}