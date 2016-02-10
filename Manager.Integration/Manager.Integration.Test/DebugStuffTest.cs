using System;
using System.IO;
using System.Linq;
using System.Threading;
using log4net;
using log4net.Config;
using Manager.Integration.Test.Helpers;
using NUnit.Framework;

namespace Manager.Integration.Test
{
    [TestFixture]
    internal class DebugStuffTest
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (DebugStuffTest));

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var task = AppDomainHelper.CreateAppDomainForManagerIntegrationConsoleHost("Debug",
                0);

            task.Start();
            JobHelper.GiveNodesTimeToInitialize();

            var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            if (AppDomainHelper.AppDomains != null &&
                AppDomainHelper.AppDomains.Any())
            {
                foreach (var appDomain in AppDomainHelper.AppDomains.Values)
                {
                    AppDomain.Unload(appDomain);
                }
            }
        }

        [Test]
        public void SleepTest()
        {
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));
                LogHelper.LogInfoWithLineNumber(i.ToString(), Logger);
            }
            
        }
    }
}