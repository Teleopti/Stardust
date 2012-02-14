using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.Events;
using Teleopti.Messaging.Protocols;
using Teleopti.Messaging.Server;

namespace Teleopti.Messaging.Tests
{

    /// <summary>
    /// The client tcp ip tests.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 25/04/2010
    /// </remarks>
    [TestFixture]
    public class ClientTcpIpTests
    {
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);
        private int _count;
        private Subscriber _subscriber;
        private MockRepository _mocks;
        private IBrokerService _brokerService;
        private IPollingManager _manager;

        /// <summary>
        /// Sets up.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 25/04/2010
        /// </remarks>
        [SetUp]
        public void SetUp()
        {
            _mocks = new MockRepository();
            _brokerService = new StubService();
        }

        /// <summary>
        /// Clients the TCP ip.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 25/04/2010
        /// </remarks>
        [Test]
        public void ClientTcpIp()
        {
            Thread subscribingThread = new Thread(StartSubscribing);
            subscribingThread.IsBackground = true;
            subscribingThread.Name = "SubscriberTests";
            subscribingThread.Start(new SocketInfo("172.22.1.31", 9090, 10));
            _resetEvent.WaitOne(1000, false);
            IPublisher publisher = new Publisher(10, MessagingProtocol.ClientTcpIP);
            publisher.StartPublishing();
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
                publisher.Send(messageInfo);
            }
            Thread.Sleep(10000);
            publisher.StopPublishing();
            _subscriber.StopSubscribing();
        }

        /// <summary>
        /// Starts the subscribing.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 25/04/2010
        /// </remarks>
        private void StartSubscribing(object state)
        {
            ISocketInfo socketInfo = (ISocketInfo)state;
            _manager = PollingManager.Instance;
            _manager.AddClient(Guid.Empty);
            _subscriber = new Subscriber(socketInfo, new PollingProtocol(_brokerService, Guid.Empty, socketInfo));
            _subscriber.EventMessageHandler += OnEventMessages;
            _subscriber.StartSubscribing(1);
        }

        /// <summary>
        /// Called when [event messages].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 25/04/2010
        /// </remarks>
        private void OnEventMessages(object sender, EventMessageArgs e)
        {
            IEventMessage message = e.Message;
            Console.WriteLine();
            Console.WriteLine("Message " + Interlocked.Increment(ref _count));
            Console.WriteLine();
            Console.WriteLine(message.ToString());
        }

    }

    public class StubService : IBrokerService
    {
        public void Initialize(IPublisher publisher, string connectionString)
        {
            throw new NotImplementedException();
        }

        public int RegisterUser(string domain, string userName)
        {
            throw new NotImplementedException();
        }

        public Guid RegisterSubscriber(int userId, string userName, int processId, string ipAddress, int port)
        {
            throw new NotImplementedException();
        }

        public void UnregisterSubscriber(IMessageInformation messageInfo)
        {
            throw new NotImplementedException();
        }

        public void UnregisterSubscriber(string address, int port)
        {
            throw new NotImplementedException();
        }

        public void UpdatePortForSubscriber(Guid subscriberId, int port)
        {
            throw new NotImplementedException();
        }

        public void UnregisterSubscriber(Guid subscriberId)
        {
            throw new NotImplementedException();
        }

        public IEventFilter RegisterFilter(Guid subscriberId, Guid domainObjectId, string domainObjectType, DateTime startDate, DateTime endDate, string userName)
        {
            throw new NotImplementedException();
        }

        public IEventFilter RegisterFilter(Guid subscriberId, Guid referenceObjectId, string referenceObjectType, Guid domainObjectId, string domainObjectType, DateTime startDate, DateTime endDate, string userName)
        {
            throw new NotImplementedException();
        }

        public void UnregisterFilter(Guid filterId)
        {
            throw new NotImplementedException();
        }

        public void SendEventMessage(DateTime eventStartDate, DateTime eventEndDate, int userId, int processId, Guid moduleId, int packageSize, bool isHeartbeat, Guid referenceObjectId, string referenceObjectType, Guid domainObjectId, string domainObjectType, DomainUpdateType updateType, string userName)
        {
            throw new NotImplementedException();
        }

        public void SendEventMessage(DateTime eventStartDate, DateTime eventEndDate, int userId, int processId, Guid moduleId, int packageSize, bool isHeartbeat, Guid domainObjectId, string domainObjectType, DomainUpdateType updateType, string userName)
        {
            throw new NotImplementedException();
        }

        public void SendEventMessage(DateTime eventStartDate, DateTime eventEndDate, int userId, int processId, Guid moduleId, int packageSize, bool isHeartbeat, Guid referenceObjectId, string referenceObjectType, Guid domainObjectId, string domainObjectType, DomainUpdateType updateType, byte[] domainObject, string userName)
        {
            throw new NotImplementedException();
        }

        public void SendEventMessage(DateTime eventStartDate, DateTime eventEndDate, int userId, int processId, Guid moduleId, int packageSize, bool isHeartbeat, Guid domainObjectId, string domainObjectType, DomainUpdateType updateType, byte[] domainObject, string userName)
        {
            throw new NotImplementedException();
        }

        public void SendEventMessages(IEventMessage[] eventMessages)
        {
            throw new NotImplementedException();
        }

        public void Log(int processId, string description, string exception, string message, string stackTrace, string userName)
        {
            throw new NotImplementedException();
        }

        public IConfigurationInfo[] RetrieveConfigurations(string configurationType)
        {
            throw new NotImplementedException();
        }

        public void SendReceipt(IEventReceipt receipt)
        {
            throw new NotImplementedException();
        }

        public void SendHeartbeat(IEventHeartbeat beat)
        {
            throw new NotImplementedException();
        }

        public string ServicePath
        {
            get { throw new NotImplementedException(); }
        }

        public int Threads
        {
            get { throw new NotImplementedException(); }
        }

        public string ConnectionString
        {
            get { throw new NotImplementedException(); }
        }

        public IList<IEventSubscriber> EventSubscriptions
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public MessagingProtocol Protocol
        {
            get { throw new NotImplementedException(); }
        }

        public IDictionary<Guid, IList<IEventFilter>> Filters
        {
            get { throw new NotImplementedException(); }
        }

        public int ClientThrottle
        {
            get { throw new NotImplementedException(); }
        }

        public int MessagingPort
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IConcurrentUsers RetrieveNumberOfConcurrentUsers(string ipAddress)
        {
            throw new NotImplementedException();
        }

        public IEventSubscriber[] RetrieveSubscribers(string ipAddress)
        {
            throw new NotImplementedException();
        }

        public IMessageInformation[] RetrieveAddresses()
        {
            throw new NotImplementedException();
        }

        public void UpdateConfigurations(IList<IConfigurationInfo> configurations)
        {
            throw new NotImplementedException();
        }

        public void DeleteConfiguration(IConfigurationInfo configurationInfo)
        {
            throw new NotImplementedException();
        }

        public void UpdateAddresses(IList<IMessageInformation> multicastAddressInfos)
        {
            throw new NotImplementedException();
        }

        public void DeleteAddresses(IMessageInformation multicastAddressInfo)
        {
            throw new NotImplementedException();
        }

        public IEventHeartbeat[] RetrieveHeartbeats()
        {
            throw new NotImplementedException();
        }

        public ILogbookEntry[] RetrieveLogbookEntries()
        {
            throw new NotImplementedException();
        }

        public IEventUser[] RetrieveEventUsers()
        {
            throw new NotImplementedException();
        }

        public IEventReceipt[] RetrieveEventReceipt()
        {
            throw new NotImplementedException();
        }

        public IEventSubscriber[] RetrieveSubscribers()
        {
            throw new NotImplementedException();
        }

        public IEventFilter[] RetrieveFilters()
        {
            throw new NotImplementedException();
        }

        public ISocketInfo RetrieveSocketInformation()
        {
            throw new NotImplementedException();
        }

        public IEventMessage[] Poll(Guid subscriberId)
        {
            return new IEventMessage[]
                {
                    new EventMessage(Guid.NewGuid(), Consts.MinDate, Consts.MaxDate, 1, 1000,
                                     Guid.NewGuid(), Consts.MaxWireLength,
                                     false, Guid.Empty,
                                     typeof (IChat).GetType().FullName, Guid.Empty,
                                     typeof (IChat).GetType().FullName,
                                     DomainUpdateType.NotApplicable, Environment.UserName,
                                     DateTime.Now)
                };
        }

    }
}
