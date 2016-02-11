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

            LogHelper.LogInfoWithLineNumber("Started TestFixtureSetUp.",
                                            Logger);

            MyTask = new Task(() =>
            {
                var path =
                    @"C:\Users\antons\Documents\Stardust\Manager.Integration\Manager.Integration.Tests.Console.Host\bin\Debug";

                var exe = "Manager.IntegrationTest.Console.Host.exe";

                var config = "Manager.IntegrationTest.Console.Host.exe.config";

                var appDomainSetup = new AppDomainSetup
                {
                    ApplicationBase = path,
                    ApplicationName = exe,
                    ConfigurationFile = config
                };

                MyAppDomain = AppDomain.CreateDomain(exe,
                                                     null,
                                                     appDomainSetup);


                LogHelper.LogInfoWithLineNumber("Started Manager.IntegrationTest.Console.Host (appdomain) with friendly name " + MyAppDomain.FriendlyName,
                                                Logger);

                MyAppDomain.ExecuteAssembly(Path.Combine(path,
                                                         exe));
            });

            MyTask.Start();

            LogHelper.LogInfoWithLineNumber("Start sleep for 90 seconds.",
                                            Logger);

            Thread.Sleep(TimeSpan.FromSeconds(90));

            LogHelper.LogInfoWithLineNumber("Finished sleep for 90 seconds.",
                                            Logger);

            LogHelper.LogInfoWithLineNumber("Finished TestFixtureSetUp.",
                                            Logger);
        }

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