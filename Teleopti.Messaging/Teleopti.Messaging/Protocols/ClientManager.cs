// ReSharper disable FieldCanBeMadeReadOnly.Local
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Logging;
using Teleopti.Messaging.Exceptions;


namespace Teleopti.Messaging.Protocols
{
    /// <summary>
    /// The client manager.
    /// </summary>
    public class ClientManager : IClientManager
    {
        private const int MaxNumberOfPorts = 5000;
        private const string TooManyPortsInUse = "Too many ports are in use.";
        private readonly IList<ClientImplementation> _clients = new List<ClientImplementation>();
        private readonly IDictionary<int, int> _portsInUse = new Dictionary<int, int>();
        private static object _lockObject = new object();
        private static IClientManager _instance;
        private IBrokerService _brokerService;
        private int _startPort;
        private int _clientThrottle;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientManager"/> class.
        /// </summary>
        private ClientManager()
        {
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static IClientManager GetInstance(IBrokerService brokerService, int startPort, int clientThrottle)
        {
            lock (_lockObject)
            {
                if (_instance == null)
                {
                    _instance = new ClientManager();
                    _instance.BrokerService = brokerService;
                    _instance.StartPort = startPort;
                    _instance.ClientThrottle = clientThrottle;
                }
            }
            return _instance;
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="value">The value.</param>
        public void QueueMessage(string address, int port, byte[] value)
        {
            lock (_lockObject)
            {
                ClientImplementation client = FindClient(port);
                if(client != null)
                    client.QueueItem(value);
            }
        }

        /// <summary>
        /// Removes the listners.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 25/04/2010
        /// </remarks>
        public void RemoveListener(int port)
        {
            IList<ClientImplementation> clientsToRemove = new List<ClientImplementation>();
            lock (_lockObject)
            {

                foreach (ClientImplementation client in _clients)
                    if (client.Port == port)
                        clientsToRemove.Add(client);

                foreach (ClientImplementation client in clientsToRemove)
                {
                    _clients.Remove(client);
                    if (_portsInUse.ContainsKey(client.Port))
                        _portsInUse.Remove(client.Port);
                    
                    client.Stop();

                }
            }
        }

        /// <summary>
        /// Gets or sets the broker service.
        /// </summary>
        /// <value>The broker service.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        public IBrokerService BrokerService
        {
            get { return _brokerService; }
            set { _brokerService = value; }
        }

        /// <summary>
        /// Gets or sets the start port.
        /// </summary>
        /// <value>The start port.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/05/2010
        /// </remarks>
        public int StartPort
        {
            get { return _startPort; }
            set { _startPort = value; }
        }

        /// <summary>
        /// Gets or sets the client throttle.
        /// </summary>
        /// <value>The client throttle.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/05/2010
        /// </remarks>
        public int ClientThrottle
        {
            get { return _clientThrottle; }
            set { _clientThrottle = value; }
        }

        /// <summary>
        /// Adds the listner.
        /// </summary>
        public int AddListener(string address)
        {
            int port;
            lock (_lockObject)
            {
                port = RetrievePort();
                try
                {
                    ClientImplementation listener = new ClientImplementation(BrokerService, address, port, _clientThrottle);
                    _clients.Add(listener);
                }
                catch (SocketException exception)
                {
                    BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exception.ToString());
                }
            }
            return port;
        }

        /// <summary>
        /// Retrieves the port.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 25/04/2010
        /// </remarks>
        private int RetrievePort()
        {
            int port = _startPort;
            for (int i = 0; i < MaxNumberOfPorts; i++)
            {
                if (_portsInUse.ContainsKey(port))
                {
                    port++;
                }
                else
                {
                    _portsInUse.Add(port, port);
                    return port;
                }
            }
            throw new PortNotAvailableException(TooManyPortsInUse);
        }

        /// <summary>
        /// Finds the client to remove.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        private ClientImplementation FindClient(int port)
        {
            foreach (ClientImplementation client in _clients)
                if (client.Port == port)
                    return client;
            return null;
        }

    }
}



// ReSharper restore FieldCanBeMadeReadOnly.Local