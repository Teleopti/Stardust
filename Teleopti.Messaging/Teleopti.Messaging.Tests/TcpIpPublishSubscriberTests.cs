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
    public class TcpIpPublishSubscriberTests
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
            subscribingThread.Start(new SocketInfo("172.22.1.31", 9090, 10));
            _resetEvent.WaitOne(1000, false);
            IPublisher publisher = new Publisher(10, MessagingProtocol.TcpIP);
            publisher.StartPublishing();
            IList<IMessageInformation> messageInfos = new List<IMessageInformation>();
            for (int i = 0; i < 30; i++)
            {
                IMessageInformation messageInfo = new MessageInformation(Guid.Empty, 0, "172.22.1.31", 9090);
                IEventMessage message = new EventMessage(Guid.NewGuid(), Consts.MinDate, Consts.MaxDate, 1, 1000,
                                                            Guid.NewGuid(), Consts.MaxWireLength, 
                                                            false, Guid.Empty,
                                                            typeof(IChat).GetType().FullName, Guid.Empty,
                                                            typeof(IChat).GetType().FullName,
                                                            DomainUpdateType.NotApplicable, Environment.UserName,
                                                            DateTime.Now);
                messageInfo.EventMessage = message;
                messageInfos.Add(messageInfo);
            }
            publisher.Send(messageInfos);
            Thread.Sleep(10000);            
        }

        private void StartSubscribing(object state)
        {
            IList<ISocketInfo> socketInfos = new List<ISocketInfo>();
            ISocketInfo socketInfo = new SocketInfo("172.22.1.31", 9090, 10);
            socketInfos.Add(socketInfo);
            ISubscriber subscriber = new Subscriber(socketInfos[0], new TcpIpProtocol(socketInfos[0]));
            subscriber.EventMessageHandler += OnEventMessages;
            subscriber.StartSubscribing(1);
        }

        

        [Test]
        public void TestTwoSubscribersSamePort()
        {
            IList<ISocketInfo> socketInfos = new List<ISocketInfo>();
            ISocketInfo socketInfo = new SocketInfo("172.22.1.31", 9090, 10);
            socketInfos.Add(socketInfo);
            ISubscriber subscriber = new Subscriber(socketInfos[0], new TcpIpProtocol(socketInfos[0]));
            subscriber.StartSubscribing(1);
            ISubscriber subscriber2 = new Subscriber(socketInfos[0], new TcpIpProtocol(socketInfos[0]));
            subscriber2.EventMessageHandler += OnEventMessages;
            subscriber2.StartSubscribing(1);
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
