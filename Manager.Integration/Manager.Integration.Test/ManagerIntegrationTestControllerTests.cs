using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using log4net;
using log4net.Config;
using Manager.Integration.Test.Helpers;
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

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            if (AppDomainHelper.AppDomains != null &&
                AppDomainHelper.AppDomains.Any())
            {
                foreach (var appDomain in AppDomainHelper.AppDomains.Values)
                {
                    try
                    {
                        AppDomain.Unload(appDomain);
                    }

                    catch (AppDomainUnloadedException)
                    {
                    }

                    catch (Exception exp)
                    {
                        LogHelper.LogErrorWithLineNumber(exp.Message,
                                                         Logger,
                                                         exp);
                    }
                }
            }
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

            var task = AppDomainHelper.CreateAppDomainForManagerIntegrationConsoleHost("Debug",
                                                                                       1);
            task.Start();
        }

        [Test]
        public async void ShouldUnloadNode1AppDomain()
        {
            LogHelper.LogInfoWithLineNumber("Start test.",
                                            Logger);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            JobHelper.GiveNodesTimeToInitialize();

            using (var client = new HttpClient())
            {
                UriBuilder uriBuilder = 
                    new UriBuilder(Properties.Settings.Default.ManagerIntegrationTestControllerBaseAddress);

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
                    new UriBuilder(Properties.Settings.Default.ManagerIntegrationTestControllerBaseAddress);

                uriBuilder.Path += "appdomain";

                var response = await client.GetAsync(uriBuilder.Uri,
                                                     cancellationTokenSource.Token);

                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();

                List<string> list =
                    JsonConvert.DeserializeObject<List<string>>(content);
            }


            Logger.Info("Finished tests.");
        }
    }
}