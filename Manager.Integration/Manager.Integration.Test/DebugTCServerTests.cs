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
        public void ManagerConsoleHostShouldNotBeInstantiated()
        {
            int numberOfIntegrationProcesses = ProcessHelper.NumberOfProcesses("Manager.IntegrationTest.Console.Host");
            Assert.IsTrue(numberOfIntegrationProcesses == 0);
        }

        [Test][Ignore]
        public void ManagerConsoleHostShouldBeInstantiated()
        {

            int numberOfIntegrationProcesses = ProcessHelper.NumberOfProcesses("Manager.IntegrationTest.Console.Host");
            Assert.IsTrue(numberOfIntegrationProcesses == 1);
        }

    }
}
