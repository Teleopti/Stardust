using System;
using System.Threading;
using NUnit.Framework;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.Interfaces.Core;
using Teleopti.Messaging.Interfaces.Events;

namespace Teleopti.Messaging.Tests
{
    [TestFixture]
    public class ProducerConsumerTest
    {
        private readonly ManualResetEvent reset = new ManualResetEvent(false);

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void BackgroundSchedulerTests()
        {
            //IQueueMessage msg = new QueueMessageTests();
            //IBackgroundQueueReader scheduler = new BackgroundQueueReader();
            //scheduler.Start("Testing");
            //scheduler.Enqueue(msg);
            //scheduler.StopReading();
        }


        [Test]
        public void ProducerConsumerQueueCountTests()
        {
            //ProducerConsumer pc = new ProducerConsumer();
            //pc.Enqueue(new QueueMessageTests());
            ////Console.WriteLine("Producer consumer count is " + pc.Count);
            //pc.Dequeue();
        }

        [Test]
        public void BackgroundSchedulerExceptionTests()
        {
            //IQueueMessage msg = new QueueMessageTestsWithException();
            //IBackgroundQueueReader scheduler = new BackgroundQueueReader();
            //EventHandler<UnhandledExceptionEventArgs> handler = new EventHandler<UnhandledExceptionEventArgs>(SchedulerErrorEvent);
            //scheduler.ErrorEvent += handler;
            //scheduler.Start("Testing");
            //scheduler.Enqueue(msg);
            //reset.WaitOne(1000, true);
            //scheduler.ErrorEvent -= handler;
        }


        private void SchedulerErrorEvent(object sender, UnhandledExceptionEventArgs e)
        {
            reset.Set();
        }


        [TearDown]
        public void TearDown()
        {
        }

    }

    public class QueueMessageTests : IQueueMessage
    {
        public void Run()
        {
        }
    }


    public class QueueMessageTestsWithException : IQueueMessage
    {
        public void Run()
        {
            throw new Exception("This tests error handling on background thread!");
        }
    }
}
