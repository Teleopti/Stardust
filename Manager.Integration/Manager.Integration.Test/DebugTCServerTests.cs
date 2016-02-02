using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Manager.Integration.Test.Helpers;
using NUnit.Framework;

namespace Manager.Integration.Test
{
    [TestFixture]
    public class DebugTCServerTests
    {
        [SetUp]
        public void SetUp()
        {
            StartManagerIntegrationConsoleHostProcess =
                    ProcessHelper.StartManagerIntegrationConsoleHostProcess(1);
            Thread.Sleep(TimeSpan.FromSeconds(10)); 
        //    ProcessHelper.ShutDownAllManagerIntegrationConsoleHostProcesses();
        ProcessHelper.ShutDownAllProcesses();
        }

        private Process StartManagerIntegrationConsoleHostProcess { get; set; }

        [Test]
        public void ManagerIntegrationConsoleHostShouldNotBeInstantiated()
        {
            int numberOfIntegrationProcesses = ProcessHelper.NumberOfProcesses("Manager.IntegrationTest.Console.Host");
            Assert.IsTrue(numberOfIntegrationProcesses == 0);
        }

        //[Test]
        //public void ManagerConsoleHostShouldNotBeInstantiated()
        //{
        //    int numberOfManagerProcesses = ProcessHelper.NumberOfProcesses("ManagerConsoleHost");
        //    Assert.IsTrue(numberOfManagerProcesses == 0);
        //}

        //[Test]
        //public void NodeConsoleHostShouldNotBeInstantiated()
        //{
        //    int numberOfNodeProcesses = ProcessHelper.NumberOfProcesses("NodeConsoleHost");
        //    Assert.IsTrue(numberOfNodeProcesses == 0);
        //}

    }
}
