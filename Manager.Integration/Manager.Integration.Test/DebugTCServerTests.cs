using System;
using System.Diagnostics;
using System.Threading;
using log4net;
using Manager.Integration.Test.Helpers;
using NUnit.Framework;

namespace Manager.Integration.Test
{
    [TestFixture]
    public class DebugTCServerTests
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (DebugTCServerTests));

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Console.WriteLine("Running TestFixtureSetUp...");
            StartManagerIntegrationConsoleHostProcess =
                ProcessHelper.StartManagerIntegrationConsoleHostProcess(1);
            Console.WriteLine("Finish with TestFixtureSetUp...");
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            ProcessHelper.ShutDownAllProcesses();
        }

        private Process StartManagerIntegrationConsoleHostProcess { get; set; }

        [Test]
        public void ManagerIntegrationConsoleHostShouldBeInstantiated()
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
            var numberOfIntegrationProcesses = ProcessHelper.NumberOfProcesses("Manager.IntegrationTest.Console.Host");
            Assert.IsTrue(numberOfIntegrationProcesses == 1);
            Thread.Sleep(TimeSpan.FromSeconds(1));
            var numberOfIntegrationProcesses1 = ProcessHelper.NumberOfProcesses("Manager.IntegrationTest.Console.Host");
            Assert.IsTrue(numberOfIntegrationProcesses1 == 1);
            Thread.Sleep(TimeSpan.FromSeconds(1));
            var numberOfIntegrationProcesses2 = ProcessHelper.NumberOfProcesses("Manager.IntegrationTest.Console.Host");
            Assert.IsTrue(numberOfIntegrationProcesses2 == 1);
            Thread.Sleep(TimeSpan.FromSeconds(1));
            var numberOfIntegrationProcesses3 = ProcessHelper.NumberOfProcesses("Manager.IntegrationTest.Console.Host");
            Assert.IsTrue(numberOfIntegrationProcesses3 == 1);
            Thread.Sleep(TimeSpan.FromSeconds(1));
            var numberOfIntegrationProcesses4 = ProcessHelper.NumberOfProcesses("Manager.IntegrationTest.Console.Host");
            Assert.IsTrue(numberOfIntegrationProcesses4 == 1);
        }

        [Test][Ignore]
        public void ShouldNotBeAnyOldProcessesLeft()
        {
            Process[] processes = Process.GetProcessesByName("Manager.IntegrationTest.Console.Host");
            Assert.IsTrue(processes.Length == 1);
        }

        //    Assert.IsTrue(numberOfManagerProcesses == 0);
        //    int numberOfManagerProcesses = ProcessHelper.NumberOfProcesses("ManagerConsoleHost");
        //{
        //public void ManagerConsoleHostShouldNotBeInstantiated()

        //[Test]
        //}

        //[Test]
        //public void NodeConsoleHostShouldNotBeInstantiated()
        //{
        //    int numberOfNodeProcesses = ProcessHelper.NumberOfProcesses("NodeConsoleHost");
        //    Assert.IsTrue(numberOfNodeProcesses == 0);
        //}
    }
}