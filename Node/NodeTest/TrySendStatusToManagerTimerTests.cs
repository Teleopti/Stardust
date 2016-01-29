using System.Threading;
using NodeTest.Fakes.Timers;
using NUnit.Framework;

namespace NodeTest
{
    [TestFixture]
    public class TrySendStatusToManagerTimerTests
    {
        readonly ManualResetEventSlim _manualResetEventSlim = new ManualResetEventSlim();


        [Test]
        public void ShouldBeAbleToInstantiateSendJobDoneTimer()
        {
            var timer = new SendJobDoneTimerFake();

            Assert.IsNotNull(timer);
        }
    }
}