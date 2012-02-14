using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.DataAccessLayer;
using Teleopti.Messaging.Server;
using Hashtable=System.Collections.Hashtable;

namespace Teleopti.Messaging.Tests
{
    [TestFixture]
    public class EndToEndTests
    {
        private const string Name = "TeleoptiBrokerService";
        private const string Port = "Port";
        private const string Server = "Server";
        private const string MessagingProtocolConstant = "MessagingProtocol";
        private const string TimeToLive = "TimeToLive";
        private BrokerService _brokerService;
        private Int32 _port;
        private string _server;
        private MessagingProtocol _messagingProtocol;
        private int _multicastPort;
        private string _connectionString;
        private int _count;

        [SetUp]
        public void SetUp()
        {
            //// Create an instance of the remote object
            _connectionString = "Data Source=teleopti457;User Id=TeleoptiDemoUser;Password=TeleoptiDemoPwd;Initial Catalog=TeleoptiAnalytics_Demo";
            InitialiseFromDatabase(_connectionString);
            // Create an instance of a channel
            BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
            serverProvider.TypeFilterLevel = TypeFilterLevel.Full;
            BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
            IDictionary properties = new Hashtable();
            properties.Add("port", _port);
            TcpChannel channel = new TcpChannel(properties, clientProvider, serverProvider);
            ChannelServices.RegisterChannel(channel, false);
            // Register as an available service
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(BrokerService), "TeleoptiBrokerService", WellKnownObjectMode.Singleton);
            _brokerService = (BrokerService)Activator.GetObject(typeof(BrokerService), String.Format(CultureInfo.InvariantCulture, "tcp://{0}:{1}/{2}", _server, _port, Name));
            InitialisePublisher(_connectionString);
            _brokerService.SendEventMessage(DateTime.Now, DateTime.Now, 1, Process.GetCurrentProcess().Id, Guid.Empty, 0, false, Guid.NewGuid(), typeof(IStartedService).AssemblyQualifiedName, DomainUpdateType.Insert, Environment.UserName);
        }

        /// <summary>
        /// Initialises the publisher.
        /// </summary>
        private void InitialisePublisher(string connectionString)
        {
            PublisherFactory factory = new PublisherFactory();
            IPublisher publisher = null;
            if (_messagingProtocol == MessagingProtocol.Multicast)
            {
                publisher = factory.CreatePublisher(50,MessagingProtocol.Multicast);
            }
            if (_messagingProtocol == MessagingProtocol.TcpIP)
            {
                publisher = factory.CreatePublisher(50, MessagingProtocol.TcpIP);
            }
            _brokerService.MessagingPort = _multicastPort;
            _brokerService.Initialize(publisher, connectionString);
        }

        private void InitialiseFromDatabase(string connectionString)
        {
            try
            {
                ConfigurationInfoReader reader = new ConfigurationInfoReader(connectionString);
                IList<IConfigurationInfo> configurationInfos = reader.Execute();
                foreach (IConfigurationInfo configurationInfo in configurationInfos)
                {
                    SetConfigurationInfo(configurationInfo);
                }

                AddressReader addressReader = new AddressReader(connectionString);
                IList<IAddressInformation> addressInfos = addressReader.Execute();
                foreach (IAddressInformation addressInfo in addressInfos)
                {
                    _multicastPort = addressInfo.Port;
                    break;
                }
            }
            catch (Exception exc)
            {
                throw new DataException("Could not connect to DataMart.", exc);
            }
        }


        private void SetConfigurationInfo(IConfigurationInfo configurationInfo)
        {
            try
            {
                if (configurationInfo.ConfigurationType == Name)
                {
                    if (configurationInfo.ConfigurationName.ToLower(CultureInfo.InvariantCulture) == Port.ToLower(CultureInfo.InvariantCulture))
                        _port = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.InvariantCulture);
                    if (configurationInfo.ConfigurationName.ToLower(CultureInfo.InvariantCulture) == Server.ToLower(CultureInfo.InvariantCulture))
                        _server = configurationInfo.ConfigurationValue;
                    if (configurationInfo.ConfigurationName.ToLower(CultureInfo.InvariantCulture) == MessagingProtocolConstant.ToLower(CultureInfo.InvariantCulture))
                        _messagingProtocol = (MessagingProtocol)Enum.Parse(typeof(MessagingProtocol), configurationInfo.ConfigurationValue.ToUpper(), true);
                }
            }
            catch (Exception exception)
            {
                throw new ArgumentException(String.Format("Could not get correct configuration from database. \r\nTeleoptiBrokerService Property that is problematic is {0}.", configurationInfo.ConfigurationName), exception);
            }
        }

        [Test, Ignore("Longrunning test")]
        public void TestTcpPublishSubscriber()
        {
            IMessageBroker mb = MessageBrokerImplementation.GetInstance("");
            mb.ExceptionHandler += new EventHandler<UnhandledExceptionEventArgs>(OnMessageBrokerUnhandledException);
            mb.StartMessageBroker();
            mb.EventMessageHandler +=new EventHandler<EventMessageArgs>(OnEventMessageRaw);
            for (int i = 0; i < 3000; i++)
            {
                SendMessage(mb, "TeleOpti");
                Thread.Sleep(100);
            }
        }

        [Test, Ignore("Longrunning test")]
        public void TestTcpPublishSubscribeEndToEnd()
        {
            IMessageBroker mb = MessageBrokerImplementation.GetInstance("");
            mb.ExceptionHandler += OnMessageBrokerUnhandledException;
            mb.RegisterEventSubscription(OnEventMessageRaw, typeof(IChat));
            mb.StartMessageBroker();
            mb.EventMessageHandler += OnEventMessageRaw;
            for (int i = 0; i < 20; i++)
            {
                SendMessage(mb, "TeleOpti");
                Thread.Sleep(100);
            }
        }


        [Test, Ignore("Longrunning test")]
        public void TestStartUpIncorrectly()
        {
            Dictionary<Type, IList<Type>> dictionary = new Dictionary<Type, IList<Type>>();
            dictionary.Add(typeof(IChat), new List<Type>());
            IMessageBroker mb = MessageBrokerImplementation.GetInstance(dictionary);
            mb.ExceptionHandler += OnMessageBrokerUnhandledException;
            mb.StartMessageBroker();
            mb.RegisterEventSubscription(OnEventMessageRaw, typeof(IChat));
            mb.EventMessageHandler += OnEventMessageRaw;
            for (int i = 0; i < 20; i++)
            {
                SendMessage(mb, "TeleOpti");
                Thread.Sleep(100);
            }
        }


        [Test, Ignore("Longrunning test")]
        public void TestMessageBrokerEndToEnd()
        {
            IMessageBroker mb = MessageBrokerImplementation.GetInstance(_connectionString);
            mb.ExceptionHandler += OnMessageBrokerUnhandledException;
            mb.StartMessageBroker();
            mb.RegisterEventSubscription(OnEventMessage, typeof(IChat));
            for (int i = 0; i < 20; i++)
            {
                SendMessage(mb, "TeleOpti");
                Thread.Sleep(100);
            }
        }

        private void OnMessageBrokerUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(sender.ToString());
            Console.WriteLine(e.ExceptionObject.ToString());
        }

        public void SendMessage(IMessageBroker broker, string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            broker.SendEventMessage(Guid.Empty, Guid.Empty, typeof(IChat), DomainUpdateType.NotApplicable, bytes);
        }

        private void OnEventMessageRaw(object sender, EventMessageArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("Message " + Interlocked.Increment(ref _count));
            Console.WriteLine(sender.ToString());
            //Console.WriteLine(e.Message);
        }

        private void OnEventMessage(object sender, EventMessageArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("Message " + Interlocked.Increment(ref _count));
            Console.WriteLine(sender.ToString());
            Console.WriteLine(e.Message);
        }


        [TearDown]
        public void TearDown()
        {

        }
    }
}
