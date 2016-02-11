using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Properties;
using Manager.Integration.Test.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Manager.Integration.Test
{
    [TestFixture]
    [Ignore]
    public class ManagerIntegrationTestControllerTests
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (ManagerIntegrationTestControllerTests));

        private const int NumberOfNodesToStart = 1;

        private bool _clearDatabase = true;
        private string _buildMode = "Debug";


        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.LogInfoWithLineNumber("Start TestFixtureTearDown",
                                            Logger);

            AppDomainTask.Dispose();

            LogHelper.LogInfoWithLineNumber("Finished TestFixtureTearDown",
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

            var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

            LogHelper.LogInfoWithLineNumber("Start TestFixtureSetUp",
                                            Logger);

            if (_clearDatabase)
            {
                DatabaseHelper.TryClearDatabase();
            }


            CancellationTokenSource = new CancellationTokenSource();

            AppDomainTask = new AppDomainTask(_buildMode);

            AppDomainTask.StartTask(CancellationTokenSource,
                                    NumberOfNodesToStart);

            JobHelper.GiveNodesTimeToInitialize();

            LogHelper.LogInfoWithLineNumber("Finshed TestFixtureSetUp",
                                            Logger);
        }

        public AppDomainTask AppDomainTask { get; set; }


        private CancellationTokenSource CancellationTokenSource { get; set; }

        [Test]
        public async void ShouldUnloadNode1AppDomain()
        {
            LogHelper.LogInfoWithLineNumber("Start test.",
                                            Logger);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            using (var client = new HttpClient())
            {
                UriBuilder uriBuilder =
                    new UriBuilder(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

                uriBuilder.Path += "appdomain/" + "Node1.config";

                var response = await client.DeleteAsync(uriBuilder.Uri,
                                                        cancellationTokenSource.Token);

                response.EnsureSuccessStatusCode();
            }

            Logger.Info("Finished tests.");
        }

        [Test]
        public async void ShouldReturnAllAppDomainKeys()
        {
            LogHelper.LogInfoWithLineNumber("Start test.",
                                            Logger);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            JobHelper.GiveNodesTimeToInitialize();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                UriBuilder uriBuilder =
                    new UriBuilder(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

                uriBuilder.Path += "appdomain";

                var response = await client.GetAsync(uriBuilder.Uri,
                                                     cancellationTokenSource.Token);

                response.EnsureSuccessStatusCode();

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

            LogHelper.LogInfoWithLineNumber("Finished test.",
                                            Logger);
        }
    }
}