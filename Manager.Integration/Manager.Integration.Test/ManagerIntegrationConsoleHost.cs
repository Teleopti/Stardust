using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net.Config;
using NUnit.Framework;

namespace Manager.Integration.Test
{
    [TestFixture()]
    [Ignore]
    public class ManagerIntegrationConsoleHost
    {
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


                MyAppDomain.ExecuteAssembly(Path.Combine(path,
                                                         exe));
            });

            MyTask.Start();

            Thread.Sleep(TimeSpan.FromSeconds(90));
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