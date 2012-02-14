// ReSharper disable FieldCanBeMadeReadOnly.Local
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Sockets;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Exceptions;


namespace Teleopti.Messaging.Protocols
{

    /// <summary>
    /// The client tcp ip connection manager.
    /// </summary>
    public class ClientTcpIpManager : IClientTcpIpManager
    {
        private static int _startPort;
        private readonly List<ICustomTcpListener> _listners = new List<ICustomTcpListener>();
        private readonly IDictionary<int, int> _portsInUse = new Dictionary<int, int>();
        private static object _lockObject = new object();
        private static IClientTcpIpManager _instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientTcpIpManager"/> class.
        /// </summary>
        private ClientTcpIpManager()
        {
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static IClientTcpIpManager GetInstance(int startPort)
        {
            lock (_lockObject)
            {
                if (_instance == null)
                {
                    _instance = new ClientTcpIpManager();
                    _startPort = startPort;
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
        public void SendPackage(string address, int port, byte[] value)
        {
            lock (_lockObject)
            {
                ICustomTcpListener customTcpListener = FindListener(port);
                if (customTcpListener != null)
                {
                    ITcpSender sender = customTcpListener.AcceptTcpSender();
                    SendPackageInternal(value, sender);
                }
            }
        }

        /// <summary>
        /// Removes the listners.
        /// </summary>
        /// <param name="eventSubscriber">The event subscriber.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 25/04/2010
        /// </remarks>
        public void RemoveListener(IEventSubscriber eventSubscriber)
        {
            IList<ICustomTcpListener> portsToRemove = new List<ICustomTcpListener>();
            lock (_lockObject)
            {

                foreach (ICustomTcpListener customTcpListener in _listners)
                    if (customTcpListener.Port == eventSubscriber.Port)
                        portsToRemove.Add(customTcpListener);

                foreach (ICustomTcpListener customTcpListener in portsToRemove)
                {
                    _listners.Remove(customTcpListener);
                    if (_portsInUse.ContainsKey(customTcpListener.Port))
                        _portsInUse.Remove(customTcpListener.Port);
                }

            }
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="sender">The sender.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void SendPackageInternal(byte[] package, ITcpSender sender)
        {
            try
            {
                NetworkStream networkStream = sender.GetStream();
                networkStream.Write(package, 0, package.Length);
                networkStream.Close();
                sender.Close();
            }
            catch (ArgumentException exc)
            {
                DeregisterSubscriber(sender.Port);
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.ToString());
            }
            catch (SocketException socketException)
            {
                ClientNotConnectedException clientNotConnected = new ClientNotConnectedException(String.Format(CultureInfo.InvariantCulture, "Client {0} on port {1} is not connected. WinSocket Error: {2}, Inner Exception: {3}.", sender.Address, sender.Port, socketException.ErrorCode, socketException.Message), socketException);
                DeregisterSubscriber(sender.Port);
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), clientNotConnected.Message);
            }
            catch (NullReferenceException nullReferenceException)
            {
                DeregisterSubscriber(sender.Port);
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), nullReferenceException.ToString());
            }
            catch (Exception exc)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.ToString());
            }
        }

        /// <summary>
        /// Removes the listner.
        /// </summary>
        /// <param name="port">The port.</param>
        public void RemoveListener(int port)
        {
            lock (_lockObject)
            {
                ICustomTcpListener listenerToRemove = FindListener(port);
                if (listenerToRemove != null)
                {

                    if (_portsInUse.ContainsKey(listenerToRemove.Port))
                        _portsInUse.Remove(listenerToRemove.Port);
                    
                    if (_listners.Contains(listenerToRemove))
                        _listners.Remove(listenerToRemove);

                    try
                    {
                        listenerToRemove.Stop();
                    }
                    catch (SocketException exception)
                    {
                        BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), String.Format(CultureInfo.InvariantCulture, "ClientTcpIpManager::RemoveListner : {0}.", exception));
                    }

                }
            }
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
                    CustomTcpListener listener = new CustomTcpListener(address, port);
                    listener.Start();
                    _listners.Add(listener);
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
            bool isPortInUse;
            int port = _startPort;
            do
            {
                if (_portsInUse.ContainsKey(port))
                {
                    port++;
                    isPortInUse = true;
                }
                else
                {
                    isPortInUse = false;
                    _portsInUse.Add(port, port);
                }
            } 
            while (isPortInUse);
            return port;
        }

        /// <summary>
        /// Deregisters the subscriber.
        /// </summary>
        /// <param name="port">The port.</param>
        private void DeregisterSubscriber(int port)
        {
            ICustomTcpListener listenerToRemove = FindListener(port);
            if (listenerToRemove != null)
            {
                _listners.Remove(listenerToRemove);
                listenerToRemove.Stop();
            }
        }

        /// <summary>
        /// Finds the listener to remove.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        private ICustomTcpListener FindListener(int port)
        {
            foreach (ICustomTcpListener listener in _listners)
                if (listener.Port == port)
                    return listener;
            return null;
        }

    }
}



// ReSharper restore FieldCanBeMadeReadOnly.Local