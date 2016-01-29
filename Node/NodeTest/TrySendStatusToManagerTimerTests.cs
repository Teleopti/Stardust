using System;
using System.Threading;
using NodeTest.Fakes.Timers;
using NUnit.Framework;
using Stardust.Node.Timers;

namespace NodeTest
{
    [TestFixture]
    public class TrySendStatusToManagerTimerTests
    {
        readonly ManualResetEventSlim _manualResetEventSlim = new ManualResetEventSlim();

        readonly Uri _fakeUrl = new Uri("http://localhost:9000");

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionWhenNodeConfigurationArgumentIsNull()
        {
            var trySendJobDoneStatusToManagerTimer = new TrySendStatusToManagerTimer(null, _fakeUrl);

            Assert.IsNotNull(trySendJobDoneStatusToManagerTimer);
        }



    }
}