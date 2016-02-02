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
            Logger.Info("Running TestFixtureSetUp...");
            StartManagerIntegrationConsoleHostProcess =
                ProcessHelper.StartManagerIntegrationConsoleHostProcess(1);
            Thread.Sleep(TimeSpan.FromSeconds(10));
            Logger.Info("Finish with TestFixtureSetUp...");
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            ProcessHelper.ShutDownAllProcesses();
        }

        private Process StartManagerIntegrationConsoleHostProcess { get; set; }

        [Test]
        public void ManagerIntegrationConsoleHostShouldNotBeInstantiated()
        {
            var numberOfIntegrationProcesses = ProcessHelper.NumberOfProcesses("Manager.IntegrationTest.Console.Host");
            Assert.IsTrue(numberOfIntegrationProcesses == 0);
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