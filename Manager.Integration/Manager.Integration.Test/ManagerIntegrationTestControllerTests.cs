using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using log4net;
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

        [Test]
        public async void Test()
        {
            Logger.Info("Started tests.");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                UriBuilder uriBuilder = new UriBuilder(Properties.Settings.Default.ManagerIntegrationTestControllerBaseAddress);

                uriBuilder.Path += "appdomains";

                var response = await client.GetAsync(uriBuilder.Uri);

                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();

                List<string> list =
                    JsonConvert.DeserializeObject<List<string>>(content);
            }


            Logger.Info("Finished tests.");
        }
    }
}