using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Manager.Integration.Test.Helpers;
using NUnit.Framework;

namespace Manager.Integration.Test
{
    [TestFixture()]
    [Ignore]
    public class ManagerIntegrationConsoleHost
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (ManagerIntegrationConsoleHost));

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.LogInfoWithLineNumber("Start TestFixtureTearDown",
                                            Logger);

            string friendlyName = MyAppDomain.FriendlyName;

            try
            {
                LogHelper.LogInfoWithLineNumber("Try unload appdomain with friendly name : " + friendlyName,
                                                Logger);

                AppDomain.Unload(MyAppDomain);

                LogHelper.LogInfoWithLineNumber("Unload appdomain with friendly name : " + friendlyName,
                                                Logger);
            }

            catch (AppDomainUnloadedException appDomainUnloadedException)
            {
                LogHelper.LogWarningWithLineNumber(appDomainUnloadedException.Message,
                                                   Logger);
            }

            catch (Exception exp)
            {
                LogHelper.LogErrorWithLineNumber(exp.Message,
                                                 Logger,
                                                 exp);
            }

            LogHelper.LogInfoWithLineNumber("Finished TestFixtureTearDown",
                                            Logger);
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

            CancellationTokenSource = new CancellationTokenSource();

            var task = AppDomainHelper.CreateAppDomainForManagerIntegrationConsoleHost("Debug",
                                                                                       1,
                                                                                       CancellationTokenSource);

            task.Start();

            JobHelper.GiveNodesTimeToInitialize();
        }

        private CancellationTokenSource CancellationTokenSource { get; set; }

        public Task MyTask { get; set; }

        [Test]
        public void Test1()
        {
            LogHelper.LogInfoWithLineNumber("Start test.",
                                            Logger);

            Assert.IsTrue(true);

            LogHelper.LogInfoWithLineNumber("Finished test.",
                                            Logger);
        }

        public AppDomain MyAppDomain { get; set; }
    }
}