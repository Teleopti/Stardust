using System;
using System.Net;
using System.Net.Sockets;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Server;

namespace Teleopti.Messaging.Client
{
    public class CustomUdpListener : ICustomUdpListener
    {
        // Fields
        private bool _active;
        private Socket _serverSocket;
        private readonly IPEndPoint _serverSocketEP;
        private readonly IPAddress _multicastAddress;

        public CustomUdpListener(int port)
        {
            _serverSocketEP = new IPEndPoint(IPAddress.Any, port);
            _serverSocket = new Socket(_serverSocketEP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            _serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        public CustomUdpListener(IPAddress multicastAddress, int port)
        {
            _multicastAddress = multicastAddress;
            _serverSocketEP = new IPEndPoint(IPAddress.Any, port);
            _serverSocket = new Socket(_serverSocketEP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            _serverSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(multicastAddress, IPAddress.Any));
            _serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        public Socket AcceptSocket()
        {
            if (!_active)
                throw new InvalidOperationException("Could not accept socket!");
            Socket retObject = _serverSocket.Accept();
            return retObject;
        }

        public IUdpSender AcceptUdpSender()
        {
            if (!_active)
            {
                throw new InvalidOperationException("Not active. Could not accept client.");
            }
            UdpSender retObject = new UdpSender(_serverSocketEP);
            return retObject;
        }


        public IUdpSender AcceptMulticastSender()
        {
            if (!_active)
            {
                throw new InvalidOperationException("Not active. Could not accept client.");
            }
            UdpSender retObject = new UdpSender(_multicastAddress, _serverSocketEP);
            return retObject;
        }

        public bool Pending()
        {
            if (!_active)
                throw new InvalidOperationException("Could not accept socket!");
            return _serverSocket.Poll(0, SelectMode.SelectRead);
        }

        public void Start()
        {
            _active = true;
        }

        public void Stop()
        {
            if (_serverSocket != null)
            {
                _serverSocket.Close();
                _serverSocket = null;
            }
            _active = false;
            _serverSocket = new Socket(_serverSocketEP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            _serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        // Properties
        protected bool Active
        {
            get { return _active; }
        }

        public bool ExclusiveAddressUse
        {
            get { return _serverSocket.ExclusiveAddressUse; }
            set 
            { 
                if (_active)
                {
                    throw new InvalidOperationException("TcpListener must be stopped.");
                }
                _serverSocket.ExclusiveAddressUse = value;
            }
        }

        public EndPoint LocalEndpoint
        {
            get
            {
                if (!_active)
                {
                    return _serverSocketEP;
                }
                return _serverSocket.LocalEndPoint;
            }
        }

        public Socket Server
        {
            get { return _serverSocket; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if(isDisposing)
            {
                Stop();
            }
        }

        #endregion

    }
}
