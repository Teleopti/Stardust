using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NodeTest
{
    [TestFixture()]
    [Ignore]
    public class NodeConsoleHostTests
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
                    @"C:\Users\antons\Documents\Stardust\Node\NodeConsoleHost\bin\Debug";

                var exe = "NodeConsoleHost.exe";

                var config = "NodeConsoleHost.exe.config";

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

            Thread.Sleep(TimeSpan.FromSeconds(60));
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