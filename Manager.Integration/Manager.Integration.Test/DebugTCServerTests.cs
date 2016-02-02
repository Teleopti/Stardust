using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Manager.Integration.Test.Helpers;
using NUnit.Framework;

namespace Manager.Integration.Test
{
    [TestFixture]
    public class DebugTCServerTests
    {
        [Test]
        public void ManagerIntegrationConsoleHostShouldNotBeInstantiated()
        {
            int numberOfIntegrationProcesses = ProcessHelper.NumberOfProcesses("Manager.IntegrationTest.Console.Host");
            Assert.IsTrue(numberOfIntegrationProcesses == 0);
        }

        [Test]
        public void ManagerConsoleHostShouldNotBeInstantiated()
        {
            int numberOfManagerProcesses = ProcessHelper.NumberOfProcesses("ManagerConsoleHost");
            Assert.IsTrue(numberOfManagerProcesses == 0);
        }

        [Test]
        public void NodeConsoleHostShouldNotBeInstantiated()
        {
            int numberOfNodeProcesses = ProcessHelper.NumberOfProcesses("NodeConsoleHost");
            Assert.IsTrue(numberOfNodeProcesses == 0);
        }

    }
}
