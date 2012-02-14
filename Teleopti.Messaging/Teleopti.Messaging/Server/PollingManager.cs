using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging;

namespace Teleopti.Messaging.Server
{
    /// <summary>
    /// The Polling Manager.
    /// </summary>
    public class PollingManager : IPollingManager
    {
        private readonly IDictionary<Guid, PollingClient> _clients = new Dictionary<Guid, PollingClient>();
        private static object _lockObject = new object();
        private static IPollingManager _instance;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PollingManager"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        private PollingManager()
        {
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static IPollingManager Instance
        {
            get 
            { 
                lock (_lockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new PollingManager();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <param name="eventMessage">The event message.</param>
        public void QueueMessage(Guid subscriberId, IEventMessage eventMessage)
        {
            lock (_lockObject)
            {
                if (subscriberId != Guid.Empty)
                {
                    PollingClient client = FindClient(subscriberId);
                    if (client != null)
                        client.QueueItem(eventMessage);
                }
            }
        }


        /// <summary>
        /// Removes the client.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        public void RemoveClient(Guid subscriberId)
        {
            lock (_lockObject)
            {
                PollingClient client = FindClient(subscriberId);
                if (client!=null)
                {
                    client.Dispose();
                    _clients.Remove(subscriberId);
                }
            }
        }

        /// <summary>
        /// Adds the client.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        public void AddClient(Guid subscriberId)
        {
            lock (_lockObject)
            {
                try
                {
                    if(subscriberId != Guid.Empty)
                        if(!_clients.ContainsKey(subscriberId))
                            _clients.Add(subscriberId, new PollingClient(subscriberId));
                }
                catch (SocketException exception)
                {
                    BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exception.ToString());
                }
            }
        }

        /// <summary>
        /// Polls the specified subscriber's queue.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        public IEventMessage[] Poll(Guid subscriberId)
        {
            IEventMessage[] eventMessages = null;
            lock(_lockObject)
            {
                PollingClient pollingClient = FindClient(subscriberId);
                if(pollingClient != null)
                    eventMessages = pollingClient.RetrieveMessages();
            }
            return eventMessages;
        }

        /// <summary>
        /// Finds the client.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <returns></returns>
        private PollingClient FindClient(Guid subscriberId)
        {
            PollingClient client;
            return _clients.TryGetValue(subscriberId,out client) ? client : null;
        }
    }
}