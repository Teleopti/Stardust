using System;
using System.Configuration;
using System.Reflection;
using System.Threading;
using System.Timers;
using NUnit.Framework;
using Stardust.Node.API;
using Stardust.Node.Interfaces;
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
        public void Test()
        {
            var jobToDo = new JobToDo
            {
                Id = Guid.NewGuid(),
                Name = "Test name",
                Serialized = "Serialized",
                Type = "NodeTest.JobHandlers.TestParams"
            };


            var asssembly = Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]);

            INodeConfiguration nodeConfiguration = new NodeConfiguration(new Uri(ConfigurationManager.AppSettings["BaseAddress"]),
                                                                         new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
                                                                         asssembly,
                                                                         ConfigurationManager.AppSettings["NodeName"]);

            TrySendJobDoneStatusToManagerTimer trySendJobDoneStatusToManagerTimer =
                new TrySendJobDoneStatusToManagerTimer(jobToDo,
                                                       nodeConfiguration,
                                                       5000);


            trySendJobDoneStatusToManagerTimer.Start();


            Thread.Sleep(TimeSpan.FromSeconds(60));
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