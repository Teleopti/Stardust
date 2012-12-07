using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Sockets;
using System.Runtime.Remoting;
using Newtonsoft.Json;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Coders;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.DataAccessLayer;
using Teleopti.Messaging.Exceptions;
using Teleopti.Messaging.Server;
using log4net;

namespace Teleopti.Messaging.Client
{
	public class MessageSender : IMessageSender
    {
		private static ILog Logger = LogManager.GetLogger(typeof(MessageSender));
        private const string Name = "TeleoptiBrokerService";
        private const string Port = "Port";
        private const string Server = "Server";
        private readonly string _connectionString;
        private IBrokerService _brokerService;
        private int _port;
        private string _server;
        private int _user;
        private string _userName;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageSender"/> class.
        /// Throws BrokerNotInstantiatedException if a BrokerService proxy could not be created.
        /// </summary>
        /// <param name="teleoptiAnalyticsConnectionString">The teleopti analytics connection string.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 10/06/2010
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "teleopti")]
        internal MessageSender(string teleoptiAnalyticsConnectionString)
        {
            if (string.IsNullOrEmpty(teleoptiAnalyticsConnectionString))
                throw new BrokerNotInstantiatedException("No connection information available in configuration file.");
            _connectionString = teleoptiAnalyticsConnectionString;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is alive.
        /// </summary>
        /// <value><c>true</c> if this instance is alive; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 10/06/2010
        /// </remarks>
        public bool IsAlive
        {
            get { return (_brokerService != null ? true : false); }
        }

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void SendRtaData(Guid personId, Guid businessUnitId, IActualAgentState actualAgentState)
		{
			if (_brokerService != null)
			{
				int sendAttempt = 0;
				while (sendAttempt < 3)
				{
					try
					{
						sendAttempt++;
						//ExternalAgentStateEncoder encoder = new ExternalAgentStateEncoder();
						//byte[] domainObject = encoder.Encode(externalAgentState);
						var domainObject = JsonConvert.SerializeObject(actualAgentState);
						_brokerService.SendEventMessage(actualAgentState.Timestamp.Add(actualAgentState.TimeInState.Negate()),
							actualAgentState.Timestamp,
							_user,
							Process.GetCurrentProcess().Id,
							Guid.Empty,
							domainObject.Length,
							false,
							personId,
							typeof(IActualAgentState).AssemblyQualifiedName,
							personId,
							typeof(IActualAgentState).AssemblyQualifiedName,
							DomainUpdateType.Insert,
							null,
							_userName);
						break;
					}
					catch (Exception)
					{
						try
						{
							Logger.ErrorFormat("Error trying to send object {0} through Message Broker. Attempt {1}.", actualAgentState, sendAttempt);
							InstantiateBrokerService();
						}
						catch (BrokerNotInstantiatedException exception)
						{
							Logger.Error("Could not reach the message broker when trying to send RTA data.", exception);
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void SendData(DateTime floor, DateTime ceiling, Guid moduleId, Guid domainObjectId, Type domainInterfaceType, DomainUpdateType updateType, string dataSource, Guid businessUnitId)
        {
            if (_brokerService != null)
            {
                int sendAttempt = 0;
                while (sendAttempt < 3)
                {
                    try
                    {
                        sendAttempt++;
                        _brokerService.SendEventMessage(floor,
                                                        ceiling,
                                                        _user,
                                                        Process.GetCurrentProcess().Id,
                                                        moduleId,
                                                        0,
														false,
														domainObjectId,
														domainInterfaceType.AssemblyQualifiedName,
                                                        domainObjectId,
                                                        domainInterfaceType.AssemblyQualifiedName,
                                                        DomainUpdateType.Insert,
                                                        _userName);
                        break;
                    }
                    catch (Exception exception)
                    {
                        Logger.Error("Trying to send event message with the MessageSender through the Message Broker failed, will try to send three times before exit.", exception);
                        InstantiateBrokerService();
                    }
                }
            }
        }

        /// <summary>
        /// Instantiates the broker service.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 09/06/2010
        /// </remarks>
        public void InstantiateBrokerService()
        {
            try
            {
                InitialiseFromDatabase(_connectionString);
                _brokerService = (BrokerService)Activator.GetObject(typeof(BrokerService), String.Format(CultureInfo.InvariantCulture, "tcp://{0}:{1}/{2}", _server, _port, Name));
                _userName = (Environment.UserName.Length > 10 ? Environment.UserName.Substring(0, 10) : Environment.UserName);
                _user = _brokerService.RegisterUser(Environment.UserDomainName, _userName);
				Logger.InfoFormat("Successfully got BrokerService proxy, connecting to server {0} on port {1}.", _server, _port);
            }
            catch (NullReferenceException ex)
            {
                throw new BrokerNotInstantiatedException("The message broker will be unavailable until this service is restarted and initialized with correct parameters", ex);
            }
            catch (ArgumentNullException ex)
            {
                throw new BrokerNotInstantiatedException("The message broker will be unavailable until this service is restarted and initialized with correct parameters", ex);
            }
            catch (RemotingException remotingException)
            {
                throw new BrokerNotInstantiatedException("The message broker will be unavailable until this service is restarted and initialized with correct parameters", remotingException);
            }
            catch (MemberAccessException memberAccessException)
            {
                throw new BrokerNotInstantiatedException("The message broker will be unavailable until this service is restarted and initialized with correct parameters", memberAccessException);
            }
            catch (SocketException socketException)
            {
                throw new BrokerNotInstantiatedException("The message broker will be unavailable until this service is restarted and initialized with correct parameters", socketException);
            }
        }

        /// <summary>
        /// Initialises from database.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 09/06/2010
        /// </remarks>
        private void InitialiseFromDatabase(string connectionString)
        {
            try
            {
                ConfigurationInfoReader reader = new ConfigurationInfoReader(connectionString);
                IList<IConfigurationInfo> configurationInfos = reader.Execute();
                foreach (IConfigurationInfo configurationInfo in configurationInfos)
                    SetConfigurationInfo(configurationInfo);
            }
            catch (Exception exc)
            {
                throw new BrokerNotInstantiatedException("Could not connect to DataMart.", exc);
            }
        }

        /// <summary>
        /// Sets the configuration info.
        /// </summary>
        /// <param name="configurationInfo">The configuration info.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 09/06/2010
        /// </remarks>
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

                }
            }
            catch (Exception exception)
            {
                throw new BrokerNotInstantiatedException(String.Format(CultureInfo.InvariantCulture, "Could not get correct configuration from database. \r\nTeleoptiBrokerService Property that is problematic is {0}.", configurationInfo.ConfigurationName), exception);
            }
        }
    }
}
