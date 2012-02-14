using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Messaging.Server
{
    public class TcpSender : ITcpSender
    {
        // Fields
        private bool _active;
        private bool _cleanedUp;
        private Socket _clientSocket;
        private NetworkStream _dataStream;
        private AddressFamily _family;
        private string _address;
        private int _port;

        // Methods
        public TcpSender() : this(AddressFamily.InterNetwork)
        {
        }

        public TcpSender(EndPoint localEP)
        {
            _family = AddressFamily.InterNetwork;
            if (localEP == null)
            {
                throw new ArgumentNullException("localEP", "Local End Point is null");
            }
            _family = localEP.AddressFamily;
            Initialize();
            SetSocketOptionsBeforeBind(Client);
            Client.Bind(localEP);
        }

        public TcpSender(AddressFamily family)
        {
            _family = AddressFamily.InterNetwork;
            if ((family != AddressFamily.InterNetwork) && (family != AddressFamily.InterNetworkV6))
            {
                throw new ArgumentException("Protocol invalid family.", "family");
            }
            _family = family;
            Initialize();
            SetSocketOptionsBeforeBind(Client);
        }

        public TcpSender(Socket acceptedSocket)
        {
            _family = AddressFamily.InterNetwork;
            Client = acceptedSocket;
            SetSocketOptionsBeforeBind(Client);
            _active = true;
        }

        public TcpSender(string hostName, int port)
        {
            _family = AddressFamily.InterNetwork;
            if (hostName == null)
            {
                throw new ArgumentNullException("hostName");
            }
            try
            {
                Connect(hostName, port);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (StackOverflowException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception)
            {
                if (_clientSocket != null)
                {
                    _clientSocket.Close();
                }
                throw;
            }
        }

        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        public void Close()
        {
            ((IDisposable)this).Dispose();
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void Connect(IPEndPoint remoteEP)
        {
            if (_cleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (remoteEP == null)
            {
                throw new ArgumentNullException("remoteEP");
            }

            // Setting socket options in order to ReuseAddress in order to try to reduce TimeWait.
            SetSocketOptionsBeforeBind(Client);

            // Connect after allowing almost everything to go wrong.
            var asyncResult = Client.BeginConnect(remoteEP,null,remoteEP);
            asyncResult.AsyncWaitHandle.WaitOne();
            Client.EndConnect(asyncResult);

            // We are now active.
            _active = true;
        }

        public void Connect(IPAddress address, int port)
        {
            if (_cleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            IPEndPoint remoteEP = new IPEndPoint(address, port);
            Connect(remoteEP);
        }

        [SuppressMessage("Microsoft.Usage", "CA2219:DoNotRaiseExceptionsInExceptionClauses")]
        public void Connect(string hostName, int port)
        {
            if (_cleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (hostName == null)
            {
                throw new ArgumentNullException("hostName");
            }
            if (_active)
            {
                throw new SocketException();
            }
            IPAddress[] hostAddresses = Dns.GetHostAddresses(hostName);
            Exception exception = null;
            Socket socket = null;
            Socket socket2 = null;
            try
            {
                if (_clientSocket == null)
                {
                    if (Socket.SupportsIPv4)
                    {
                        socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        SetSocketOptionsBeforeBind(socket2);
                    }
                    if (Socket.OSSupportsIPv6)
                    {
                        socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                        SetSocketOptionsBeforeBind(socket);
                    }
                }
                foreach (IPAddress address in hostAddresses)
                {
                    try
                    {
                        if (_clientSocket == null)
                        {
                            if ((address.AddressFamily == AddressFamily.InterNetwork) && (socket2 != null))
                            {
                                socket2.Connect(address, port);
                                _clientSocket = socket2;
                                if (socket != null)
                                {
                                    socket.Close();
                                }
                            }
                            else if (socket != null)
                            {
                                socket.Connect(address, port);
                                _clientSocket = socket;
                                if (socket2 != null)
                                {
                                    socket2.Close();
                                }
                            }
                            _family = address.AddressFamily;
                            _active = true;
                        }
                        if (address.AddressFamily == _family)
                        {
                            Connect(new IPEndPoint(address, port));
                            _active = true;
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                }
            }
            catch (Exception exception3)
            {
                if (((exception3 is ThreadAbortException) || (exception3 is StackOverflowException)) || (exception3 is OutOfMemoryException))
                {
                    throw;
                }
                exception = exception3;
            }
            finally
            {
                if (!_active)
                {
                    if (socket != null)
                    {
                        socket.Close();
                    }
                    if (socket2 != null)
                    {
                        socket2.Close();
                    }
                    if (exception != null)
                    {
                        throw exception;
                    }
                    throw new SocketException();
                }
            }
        }

        public void Connect(IPAddress[] ipAddresses, int port)
        {
            var asyncResult = Client.BeginConnect(ipAddresses, port, null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
            Client.EndConnect(asyncResult);
            _active = true;
        }

        public void Disconnect()
        {
            var asyncResult = Client.BeginDisconnect(true,null,true);
            asyncResult.AsyncWaitHandle.WaitOne();
            Client.EndDisconnect(asyncResult);
        }

        protected virtual void Dispose(bool disposing)
        {            
            if (disposing)
            {
                IDisposable dataStream = _dataStream;
                if (dataStream != null)
                {
                    dataStream.Dispose();
                }
                else
                {
                    Socket client = Client;
                    if (client != null)
                    {
                        client.Close();
                        Client = null;                        
                    }
                }
            }
            _cleanedUp = true;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public NetworkStream GetStream()
        {
            if (_cleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (!Client.Connected)
            {
                throw new InvalidOperationException("Sender not connected.");
            }
            if (_dataStream == null)
            {
                _dataStream = new NetworkStream(Client, true);
            }
            return _dataStream;
        }

        private void Initialize()
        {
            Client = new Socket(_family, SocketType.Stream, ProtocolType.Tcp);
            _active = false;
        }

        private void SetSocketOptionsBeforeBind(Socket socket)
        {
            if (Client != null)
            {
                // Set option that allows socket to connect to address and port already in use.
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }
        }

        private int NumericOption(SocketOptionLevel optionLevel, SocketOptionName optionName)
        {
            return (int)Client.GetSocketOption(optionLevel, optionName);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Properties
        protected bool Active
        {
            get { return _active; }
            set { _active = value; }
        }

        public int Available
        {
            get { return _clientSocket.Available; }
        }

        public Socket Client
        {
            get { return _clientSocket; }
            set { _clientSocket = value; }
        }

        public bool Connected
        {
            get { return _clientSocket.Connected; }
        }

        public bool ExclusiveAddressUse
        {
            get { return _clientSocket.ExclusiveAddressUse; }
            set { _clientSocket.ExclusiveAddressUse = value; }
        }

        public LingerOption LingerState
        {
            get { return (LingerOption)Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger); }
            set { Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, value); }
        }

        public bool NoDelay
        {
            get
            {
                if (NumericOption(SocketOptionLevel.Tcp, SocketOptionName.Debug) == 0)
                {
                    return false;
                }
                return true;
            }
            set { Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug, value ? 1 : 0); }
        }

        public int ReceiveBufferSize
        {
            get { return NumericOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer); }
            set { Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, value); }
        }

        public int ReceiveTimeout
        {
            get { return NumericOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout); }
            set { Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, value); }
        }

        public int SendBufferSize
        {
            get { return NumericOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer); }
            set { Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, value); }
        }

        public int SendTimeout
        {
            get { return NumericOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout); }
            set { Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, value); }
        }

        public void Shutdown()
        {
            Client.Shutdown(SocketShutdown.Both);
        }

    }
}
