using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.Events;
using Teleopti.Messaging.Protocols;
using Teleopti.Messaging.Server;

namespace Teleopti.Messaging.Tests
{
    [TestFixture]
    public class UdpTests
    {
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);
        private int _count;

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void SubscriberTests()
        {
            Thread subscribingThread = new Thread(StartSubscribing);
            subscribingThread.IsBackground = true;
            subscribingThread.Name = "SubscriberTests";
            subscribingThread.Start(new SocketInfo("192.168.0.3", 9090, 10));
            _resetEvent.WaitOne(1000, false);
            IPublisher publisher = new Publisher(10, MessagingProtocol.Udp);
            publisher.StartPublishing();
            for (int i = 0; i < 10; i++)
            {
                IMessageInformation messageInfo = new MessageInformation(Guid.Empty, 0, "192.168.0.3", 9090, 1);
                IEventMessage message = new EventMessage(Guid.NewGuid(), Consts.MinDate, Consts.MaxDate, 1, 1000,
                                            Guid.NewGuid(), Consts.MaxWireLength,
                                            false, Guid.Empty,
                                            typeof(IChat).GetType().FullName, Guid.Empty,
                                            typeof(IChat).GetType().FullName,
                                            DomainUpdateType.NotApplicable, Environment.UserName,
                                            DateTime.Now);
                messageInfo.EventMessage = message;
                publisher.Send(messageInfo);
                Thread.Sleep(500);
            }
        }

        private void StartSubscribing(object state)
        {
            IList<ISocketInfo> socketInfos = new List<ISocketInfo>();
            ISocketInfo socketInfo = new SocketInfo("172.22.1.31", 9090, 10);
            socketInfos.Add(socketInfo);
            ISubscriber subscriber = new Subscriber(socketInfos[0], new UdpProtocol(socketInfos[0]));
            subscriber.EventMessageHandler += OnEventMessages;
            subscriber.StartSubscribing(1);
        }

        private void OnEventMessages(object sender, EventMessageArgs e)
        {
            IEventMessage message = e.Message;
            Console.WriteLine();
            Console.WriteLine("Message " + Interlocked.Increment(ref _count));
            Console.WriteLine();
            Console.WriteLine(message.ToString());
        }

        [TearDown]
        public void TearDown()
        {
        }


    }
}
