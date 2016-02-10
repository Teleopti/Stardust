using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Stardust.Manager
{
    [TestFixture()]
    [Ignore]
    public class ManagerConsoleHostTests
    {
        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            AppDomain.Unload(MyAppDomain);
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
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