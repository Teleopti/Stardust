using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using NUnit.Framework;
using Teleopti.Interfaces.MessageBroker.Coders;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Coders;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.Events;
using Teleopti.Messaging.Protocols;
using Teleopti.Messaging.Server;

namespace Teleopti.Messaging.Tests
{
    [TestFixture]
    public class TransportTest
    {
        private static readonly object _lockObject = new object();
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);
        private readonly EventHandler<EventMessageArgs> _messageHandler;
        private readonly EventHandler<UnhandledExceptionEventArgs> _exceptionHandler;
        
        private IPublisher _publisher;
        private ISubscriber _subscriber;
        private readonly IList<IEventMessage> _messages = new List<IEventMessage>();
        private int _count;

        public TransportTest()
        {
            _messageHandler = OnEventMessage;
            _exceptionHandler = OnUnhandledException;
        }

        [SetUp]
        public void SetUp()
        {
        }


        [Test]
        public void IPAddressByHostName()
        {
            const string hostname = "teleopti457";
            string ipAddress = SocketUtility.GetIPAddressByHostName(hostname);
            Console.WriteLine(ipAddress);
        }

        [Test]
        public void DateTimeTest()
        {
            DateTime dt = DateTime.Now;
            Console.WriteLine(dt.ToString());
        }

        [Test]
        public void SubscriberTests()
        {

            //"235.235.235.240"
            IList<ISocketInfo> socketsPublishing = new List<ISocketInfo>();
            socketsPublishing.Add(new SocketInfo("235.235.235.240", 8050, 10));
            _publisher = new Publisher(10, MessagingProtocol.Multicast);
            _publisher.UnhandledExceptionHandler += OnUnhandledException;
            _publisher.StartPublishing();

            Thread subscribingThread = new Thread(StartSubscribing);
            subscribingThread.IsBackground = true;
            subscribingThread.Name = "SubscriberTests";
            subscribingThread.Start(new SocketInfo("235.235.235.240", 8050, 10));
            _resetEvent.WaitOne(1000, false);

            IMessageInformation messageInformation = new MessageInformation(Guid.Empty, 1, "235.235.235.240", 8050, 1);
            IEventMessage message = new EventMessage();
            message.EventId = Guid.Empty;
            message.UserId = 1;
            message.DomainObjectId = Guid.NewGuid();
            message.DomainObjectType = typeof(Object).AssemblyQualifiedName;
            message.DomainObject = new byte[0];
            message.ChangedBy = "ankarlp";
            message.ChangedDateTime = DateTime.Now;
            messageInformation.EventMessage = message;
            IEventMessageEncoder encoder = new EventMessageEncoder();
            messageInformation.Package = encoder.Encode(message);

            _publisher.Send(messageInformation);
            _resetEvent.WaitOne(1000, false);
            _subscriber.Dispose();
            _publisher.Dispose();

        }

        [Test]
        public void SubscriberSendNullTests()
        {
            IList<ISocketInfo> socketsPublishing = new List<ISocketInfo>();
            socketsPublishing.Add(new SocketInfo("235.235.235.232", 9092, 10));
            _publisher = new Publisher(10, MessagingProtocol.Multicast);
            _publisher.UnhandledExceptionHandler += OnUnhandledException;
            _publisher.StartPublishing();

            Thread subscribingThread = new Thread(StartSubscribing)
            {
                IsBackground = true,
                Name = "SubscriberTests"
            };

            subscribingThread.Start(new SocketInfo("235.235.235.232", 9092, 10));
            _resetEvent.WaitOne(1000, false);
            _publisher.Send((IMessageInformation) null);
            _resetEvent.WaitOne(1000, false);
            _subscriber.EventMessageHandler -= _messageHandler;
            _subscriber.UnhandledExceptionHandler -= _exceptionHandler;
            _publisher.UnhandledExceptionHandler -= _exceptionHandler;
            _subscriber.Dispose();
            _publisher.Dispose();
        }

        [Test]
        public void SubscriberShutDownTests()
        {
            IList<ISocketInfo> socketsPublishing = new List<ISocketInfo>();
            socketsPublishing.Add(new SocketInfo("235.235.235.233", 9093, 10));
            _publisher = new Publisher(10, MessagingProtocol.Multicast);
            _publisher.UnhandledExceptionHandler += OnUnhandledException;
            _publisher.StartPublishing();
            Thread subscribingThread = new Thread(StartSubscribing)
                                           {
                                               IsBackground = true,
                                               Name = "SubscriberTests"
                                           };
            subscribingThread.Start(new SocketInfo("235.235.235.233", 9093, 10));
            _resetEvent.WaitOne(1000, false);
            _subscriber.EventMessageHandler -= _messageHandler;
            _subscriber.UnhandledExceptionHandler -= _exceptionHandler;
            _publisher.UnhandledExceptionHandler -= _exceptionHandler;
            _subscriber.Dispose();
            _publisher.Dispose();
        }

        [Test, Ignore("Run this test manually, several dependencies.")]
        public void EndtoEndTests()
        {
            // Create an instance of a channel
            TcpChannel channel = new TcpChannel(9080);
            ChannelServices.RegisterChannel(channel, false);

            // Register as an available service
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(BrokerService), "TeleoptiBrokerService", WellKnownObjectMode.Singleton);

            // Create an instance of the remote object
            BrokerService brokerService = (BrokerService)Activator.GetObject(typeof(BrokerService), String.Format(CultureInfo.InvariantCulture, "tcp://{0}:{1}/{2}", "172.22.1.31", 9080, "TeleoptiBrokerService"));
            IMessageBroker broker = MessageBrokerImplementation.GetInstance();
            broker.EventMessageHandler += OnEventMessage;
            broker.ExceptionHandler += OnUnhandledException;
            _resetEvent.WaitOne(2000, false);
            brokerService.SendEventMessage(DateTime.Now, DateTime.Now, 1, Process.GetCurrentProcess().Id, Guid.Empty, 0, true, Guid.NewGuid(), typeof(EventHeartbeat).AssemblyQualifiedName, DomainUpdateType.Insert, "ankarlp");
            _resetEvent.WaitOne(2000, false);
        }

        private void StartSubscribing(object obj)
        {
            IList<ISocketInfo> socketsPublishing = new List<ISocketInfo>();
            SocketInfo info = (SocketInfo)obj;
            socketsPublishing.Add(info);
            _subscriber = new Subscriber(socketsPublishing[0], new MulticastProtocol(socketsPublishing[0]));
            _subscriber.EventMessageHandler += _messageHandler;
            _subscriber.UnhandledExceptionHandler += OnUnhandledException;
            _subscriber.StartSubscribing(1);
        }

        // ReSharper disable MemberCanBeMadeStatic
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }
        // ReSharper restore MemberCanBeMadeStatic

        private void OnEventMessage(object sender, EventMessageArgs e)
        {
            lock (_lockObject)
            {
                _messages.Add(e.Message);
                IEventMessage message = e.Message;
                Console.WriteLine();
                Console.WriteLine("Message " + Interlocked.Increment(ref _count));
                Console.WriteLine();
                Console.WriteLine(message.ToString());
            }
        }

        [TearDown]
        public void TearDown()
        {
        }

    }
}
