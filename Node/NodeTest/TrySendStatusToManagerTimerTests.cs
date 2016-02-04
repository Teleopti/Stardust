using System;
using System.Configuration;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Stardust.Node.API;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;

namespace NodeTest
{
    [TestFixture]
    public class TrySendStatusToManagerTimerTests
    {
        readonly ManualResetEventSlim _manualResetEventSlim = new ManualResetEventSlim();

        readonly Uri _fakeUrl = new Uri("http://localhost:9000/jobmanager/");

        private INodeConfiguration _nodeConfiguration;

        private Uri FakeUrl
        {
            get { return _fakeUrl; }
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var baseAddress = new Uri(ConfigurationManager.AppSettings["BaseAddress"]);

            var managerLocation = new Uri(ConfigurationManager.AppSettings["ManagerLocation"]);

            var handlerAssembly = Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]);

            var nodeName = ConfigurationManager.AppSettings["NodeName"];

            _nodeConfiguration = new NodeConfiguration(baseAddress,
                                                        managerLocation, 
                                                        handlerAssembly,
                                                        nodeName);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ShouldThrowExceptionWhenNodeConfigurationArgumentIsNull()
        {
            var trySendJobDoneStatusToManagerTimer = new TrySendStatusToManagerTimer(null,
                                                                                     FakeUrl);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionWhenCallBackTemplateUriArgumentIsNull()
        {
            var trySendJobDoneStatusToManagerTimer = new TrySendStatusToManagerTimer(_nodeConfiguration,
                                                                                     null);

        }

    }
}