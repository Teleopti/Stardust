using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using NUnit.Framework;
using Stardust.Manager.Helpers;

namespace ManagerTest
{
    [TestFixture()]
    [Ignore]
    public class ManagerConsoleHostTests
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ManagerConsoleHostTests));

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            AppDomain.Unload(MyAppDomain);
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

            LogHelper.LogInfoWithLineNumber(Logger,string.Empty);

            MyTask = new Task(() =>
            {
                var path =
                    @"C:\Users\antons\Documents\Stardust\Manager\ManagerConsoleHost\bin\Debug";

                var exe = "ManagerConsoleHost.exe";

                var config = "ManagerConsoleHost.exe.config";

                var appDomainSetup = new AppDomainSetup
                {
                    ApplicationBase = path,
                    ApplicationName = exe,
                    ConfigurationFile = config
                };

                MyAppDomain = AppDomain.CreateDomain(exe,
                                                     null,
                                                     appDomainSetup);


                MyAppDomain.ExecuteAssembly(Path.Combine(path,
                                                         exe));
            });

            MyTask.Start();

            Thread.Sleep(TimeSpan.FromSeconds(15));
        }

        public Task MyTask { get; set; }

        [Test]
        public void Test1()
        {
            Assert.IsTrue(true);
        }

        public AppDomain MyAppDomain { get; set; }
    }
}