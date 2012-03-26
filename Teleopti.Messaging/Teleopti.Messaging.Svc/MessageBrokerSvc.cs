using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.ServiceProcess;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.DataAccessLayer;
using Teleopti.Messaging.Server;

namespace Teleopti.Messaging.Svc
{
    public partial class MessageBrokerSvc : ServiceBase
    {
        private const string Name = "TeleoptiBrokerService";
        private const string Port = "Port";
        private const string Server = "Server";
        private const string MessagingProtocolConstant = "MessagingProtocol";
        private const string TimeToLive = "TimeToLive";
        private const string ServerThrottle = "ServerThrottle";

        private int _messagingPort;
        private int _timeToLive;
        private int _port;
        private int _serverThrottle;
        private string _server;
        private string _connectionString;
        private string _multicastAddress;
        private BrokerService _brokerService;
        private MessagingProtocol _messagingProtocol;

        public MessageBrokerSvc()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _connectionString = ConfigurationManager.AppSettings["MessageBroker"];
            // Create an instance of the remote object
            InitialiseFromDatabase(_connectionString);
            //Debugger.Launch();
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
        	_brokerService.SendEventMessage(DateTime.Now, DateTime.Now, 1, Process.GetCurrentProcess().Id, Guid.Empty, 0,
        	                                false, Guid.NewGuid(), typeof (IStartedService).AssemblyQualifiedName,
        	                                Guid.NewGuid(), typeof (IStartedService).AssemblyQualifiedName,
        	                                DomainUpdateType.Insert, Environment.UserName);
        }

        /// <summary>
        /// Initialises the publisher.
        /// </summary>
        private void InitialisePublisher(string connectionString)
        {
            PublisherFactory factory = new PublisherFactory();
            IPublisher publisher = factory.CreatePublisher(_serverThrottle, _messagingProtocol);
            _brokerService.MessagingPort = _messagingPort;
            _brokerService.MulticastAddress = _multicastAddress;
            _brokerService.TimeToLive = _timeToLive;
            _brokerService.Initialize(publisher, connectionString);
        }

        /// <summary>
        /// Initialises from database.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
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
                    _multicastAddress = SocketUtility.IsIpAddress(addressInfo.Address) ? addressInfo.Address : SocketUtility.GetIPAddressByHostName(addressInfo.Address);
                    _messagingPort = addressInfo.Port;
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

                    if (configurationInfo.ConfigurationName.ToUpperInvariant() == Port.ToUpperInvariant())
                        _port = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.InvariantCulture);

                    if (configurationInfo.ConfigurationName.ToUpperInvariant() == Server.ToUpperInvariant())
                        _server = SocketUtility.IsIpAddress(configurationInfo.ConfigurationValue) ? configurationInfo.ConfigurationValue : SocketUtility.GetIPAddressByHostName(configurationInfo.ConfigurationValue);

                    if (configurationInfo.ConfigurationName.ToUpperInvariant() == MessagingProtocolConstant.ToUpperInvariant())
                        _messagingProtocol = (MessagingProtocol)Enum.Parse(typeof(MessagingProtocol), configurationInfo.ConfigurationValue.ToUpperInvariant(), true);

                    if (configurationInfo.ConfigurationName.ToUpperInvariant() == TimeToLive.ToUpperInvariant())
                        _timeToLive = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.InvariantCulture);

                    if (configurationInfo.ConfigurationName.ToUpperInvariant() == ServerThrottle.ToUpperInvariant())
                        _serverThrottle = Convert.ToInt32(configurationInfo.ConfigurationValue, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception exception)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Could not get correct configuration from database. \r\nTeleoptiBrokerService Property that is problematic is {0}.", configurationInfo.ConfigurationName), exception);
            }
        }

        protected override void OnStop()
        {
            _brokerService.Dispose();
        }

    }
}
