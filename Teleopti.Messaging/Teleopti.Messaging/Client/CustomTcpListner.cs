using System;
using System.Net;
using System.Net.Sockets;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Server;

namespace Teleopti.Messaging.Client
{
    /// <summary>
    /// The custom tcp ip listner.
    /// </summary>
    public class CustomTcpListener : ICustomTcpListener
    {
        // Fields
        private bool _active;
        //private bool _exclusiveAddressUse;
        private Socket _serverSocket;
        private readonly IPEndPoint _serverSocketEP;
        private int _port;
        private string _address;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomTcpListener"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        public CustomTcpListener(int port)
        {
            _port = port;
            _serverSocketEP = new IPEndPoint(IPAddress.Any, port);
            _serverSocket = new Socket(_serverSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            SetServerSocketOptionsBeforeBind();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomTcpListener"/> class.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        public CustomTcpListener(string address, int port)
        {
            _port = port;
            _address = address;
            _serverSocketEP = new IPEndPoint(IPAddress.Any, port);
            _serverSocket = new Socket(_serverSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            SetServerSocketOptionsBeforeBind();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomTcpListener"/> class.
        /// </summary>
        /// <param name="localEP">The local EP.</param>
        public CustomTcpListener(IPEndPoint localEP)
        {
            if (!_active)
                throw new InvalidOperationException("Could not accept socket!");
            _serverSocketEP = localEP;
            _serverSocket = new Socket(_serverSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            SetServerSocketOptionsBeforeBind();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomTcpListener"/> class.
        /// </summary>
        /// <param name="localAddress">The local address.</param>
        /// <param name="port">The port.</param>
        public CustomTcpListener(IPAddress localAddress, int port)
        {
            if (!_active)
                throw new InvalidOperationException("Could not accept socket!");

            _port = port;
            _serverSocketEP = new IPEndPoint(localAddress, port);
            _serverSocket = new Socket(_serverSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            SetServerSocketOptionsBeforeBind();
        }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        /// <summary>
        /// Accepts the socket.
        /// </summary>
        /// <returns></returns>
        public Socket AcceptSocket()
        {
            if (!_active)
                throw new InvalidOperationException("Could not accept socket!");
            var asyncResult = _serverSocket.BeginAccept(null,null);
            asyncResult.AsyncWaitHandle.WaitOne();
            Socket retObject = _serverSocket.EndAccept(asyncResult);
            return retObject;
        }

        /// <summary>
        /// Accepts the TCP sender.
        /// </summary>
        /// <returns></returns>
        public IAsyncResult BeginAcceptTcpSender(AsyncCallback callback)
        {
            if (!_active)
            {
                throw new InvalidOperationException("Not active. Could not accept client.");
            }

            return _serverSocket.BeginAccept(callback,_serverSocket);
        }

        public ITcpSender EndAcceptTcpSender(IAsyncResult asyncResult)
        {
            var serverSocket = (Socket)asyncResult.AsyncState;
            var socket = serverSocket.EndAccept(asyncResult);

            TcpSender retObject = new TcpSender(socket);
            retObject.Address = _address;
            retObject.Port = _port;
            return retObject;
        }


        /// <summary>
        /// Pendings this instance.
        /// </summary>
        /// <returns></returns>
        public bool Pending()
        {
            if (!_active)
                throw new InvalidOperationException("Could not accept socket!");
            return _serverSocket.Poll(0, SelectMode.SelectRead);
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            Start(0x7fffffff);
        }

        /// <summary>
        /// Starts the specified backlog.
        /// </summary>
        /// <param name="backlog">The backlog.</param>
        public void Start(int backlog)
        {
            if ((backlog > 0x7fffffff) || (backlog < 0))
            {
                throw new ArgumentOutOfRangeException("backlog");
            }
            if (_serverSocket == null)
            {
                throw new InvalidOperationException("InvalidSocketHandle");
            }
            if (!_active)
            {

                SetServerSocketOptionsBeforeBind();

                _serverSocket.Bind(_serverSocketEP);
                _serverSocket.Listen(backlog);
                _active = true;
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            if (_serverSocket != null)
            {
                _serverSocket.Close();
                _serverSocket = null;
            }
            _active = false;
            _serverSocket = new Socket(_serverSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
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


        private void SetServerSocketOptionsBeforeBind()
        {
            if (_serverSocket != null)
            {
                // Set option that allows socket to connect to address and port already in use.
                _serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }
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
