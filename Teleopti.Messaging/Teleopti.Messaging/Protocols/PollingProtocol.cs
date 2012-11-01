using System;
using System.Net.Sockets;
using System.Runtime.Remoting;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Server;
using log4net;

namespace Teleopti.Messaging.Protocols
{
    /// <summary>
    /// The client initiated tcp/ip protocol.
    /// </summary>
    public class PollingProtocol : Protocol
    {
		private static ILog Logger = LogManager.GetLogger(typeof(PollingProtocol));

        /// <summary>
        /// Initializes a new instance of the <see cref="PollingProtocol"/> class.
        /// </summary>
        /// <param name="brokerService">The broker service.</param>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <param name="socketInformation">The socket information.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        public PollingProtocol(IBrokerService brokerService, Guid subscriberId, ISocketInfo socketInformation) : base(brokerService, subscriberId, socketInformation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PollingProtocol"/> class.
        /// </summary>
        /// <param name="brokerService">The broker service.</param>
        /// <param name="subscriberId">The subscriber id.</param>
        public PollingProtocol(IBrokerService brokerService, Guid subscriberId) : base(brokerService, subscriberId)
        {
        }

        /// <summary>
        /// Sends the message server side by queueing the message up for collection.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        public override void SendPackage(IEventMessage eventMessage)
        {
            PollingManager.Instance.QueueMessage(SubscriberId, eventMessage);
        }

        /// <summary>
        /// This method is called client side and handles all incomming messages from the server
        /// through it's base class method called, HandleEventMessage.
        /// </summary>
        public override void ReadByteStream()
        {
            if (BrokerService != null)
            {
                try
                {
                    IEventMessage[] eventMessages = BrokerService.Poll(SubscriberId);
                    MessagingHandlerPool.QueueUserWorkItem(HandleEventMessage, eventMessages);
                }
                catch (SocketException socketException)
                {
                    Logger.Error("Read byte stream error.", socketException);
                }
                catch (RemotingException remotingException)
                {
                    Logger.Error("Read byte stream error.", remotingException);
                }
            }
        }

    }
}