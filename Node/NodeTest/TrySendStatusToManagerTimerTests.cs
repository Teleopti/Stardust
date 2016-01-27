using System;
using System.Threading;
using System.Timers;
using NUnit.Framework;
using Stardust.Node.Timers;

namespace NodeTest
{
    [TestFixture]
    public class TrySendStatusToManagerTimerTests
    {
        public bool TimerOnTrySendStatusSuccededTriggered { get; set; }

        private void TimerOnTrySendStatusSucceded(object sender,
            EventArgs eventArgs)
        {
            TimerOnTrySendStatusSuccededTriggered = true;
        }

        private void OverrideElapsedEventHandler(object sender,
            ElapsedEventArgs elapsedEventArgs)
        {
            var trySendStatusToManagerTimer = sender as TrySendStatusToManagerTimer;

            if (trySendStatusToManagerTimer != null)
            {
                trySendStatusToManagerTimer.TriggerTrySendStatusSucceded();
            }
        }

        [Test]
        public void ShouldNotBeStartedImplicitly()
        {
            var timer = new TrySendStatusToManagerTimer(null,
                null,
                null,
                OverrideElapsedEventHandler);

            Assert.IsTrue(timer.Enabled == false);
        }

        [Test]
        public void ShouldTriggerSuccessEvent()
        {
            TimerOnTrySendStatusSuccededTriggered = false;

            var timer = new TrySendStatusToManagerTimer(null,
                null,
                null,
                OverrideElapsedEventHandler,
                1000);

            timer.TrySendStatusSucceded += TimerOnTrySendStatusSucceded;

            timer.Start();
            Thread.Sleep(TimeSpan.FromSeconds(2)); //Wait for timer (takes 1 second)

            Assert.IsTrue(TimerOnTrySendStatusSuccededTriggered);
        }
    }
}