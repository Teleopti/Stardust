using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Messaging.Server
{
    /// <summary>
    /// The UDP Sender.
    /// </summary>
    public class UdpSender : IUdpSender
    {
        // Fields
        private bool _active;
        private bool _cleanedUp;
        private Socket _clientSocket;
        private AddressFamily _family;

        // Methods
        public UdpSender() : this(AddressFamily.InterNetwork)
        {
        }

        public UdpSender(int port) : this(port, AddressFamily.InterNetwork)
        {
        }

        // ReSharper disable SuggestBaseTypeForParameter
        
        public UdpSender(IPEndPoint localEP)
        {
            _family = AddressFamily.InterNetwork;
            if (localEP == null)
                throw new ArgumentNullException("localEP");
            _family = localEP.AddressFamily;
            CreateClientSocket();
            Client.Bind(localEP);
        }

        public UdpSender(IPAddress multicastAddress, IPEndPoint localEP)
        {
            _family = AddressFamily.InterNetwork;
            if (localEP == null)
                throw new ArgumentNullException("localEP");
            _family = localEP.AddressFamily;
            CreateClientSocket();
            Client.Bind(localEP);
            JoinMulticastGroup(multicastAddress);
        }

        // ReSharper restore SuggestBaseTypeForParameter

        public UdpSender(AddressFamily family)
        {
            _family = AddressFamily.InterNetwork;
            if ((family != AddressFamily.InterNetwork) && (family != AddressFamily.InterNetworkV6))
            {
                throw new ArgumentException("Invalid family", "family");
            }
            _family = family;
            CreateClientSocket();
        }

        public UdpSender(int port, AddressFamily family)
        {
            _family = AddressFamily.InterNetwork;
            if ((family != AddressFamily.InterNetwork) && (family != AddressFamily.InterNetworkV6))
            {
                throw new ArgumentException("Invalid family", "family");
            }
            _family = family;
            IPEndPoint point = _family == AddressFamily.InterNetwork ? new IPEndPoint(IPAddress.Any, port) : new IPEndPoint(IPAddress.IPv6Any, port);
            CreateClientSocket();
            Client.Bind(point);
        }

        public UdpSender(string hostName, int port)
        {
            _family = AddressFamily.InterNetwork;
            if (hostName == null)
            {
                throw new ArgumentNullException("hostName");
            }
            Connect(hostName, port);
        }

        public UdpSender(Socket acceptedSocket)
        {
            _family = AddressFamily.InterNetwork;
            Client = acceptedSocket;
            _active = true;
        }

        public void Close()
        {
            Dispose(true);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "endPoint")]
        public void Connect(IPEndPoint endPoint)
        {
            if (_cleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (endPoint == null)
            {
                throw new ArgumentNullException("endPoint");
            }
            Client.Connect(endPoint);
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
            IPEndPoint endPoint = new IPEndPoint(address, port);
            Connect(endPoint);
        }

        [SuppressMessage("Microsoft.Usage", "CA2219:DoNotRaiseExceptionsInExceptionClauses"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
                        socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    }
                    if (Socket.OSSupportsIPv6)
                    {
                        socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
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
                            return;
                        }
                        if (address.AddressFamily == _family)
                        {
                            Connect(new IPEndPoint(address, port));
                            _active = true;
                            return;
                        }
                    }
                    catch (Exception exception2)
                    {
                        exception = exception2;
                    }
                }
            }
            catch (Exception exception3)
            {
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
                    throw new SocketException((int) SocketError.NotConnected);
                }
            }
        }

        private void CreateClientSocket()
        {
            Client = new Socket(_family, SocketType.Dgram, ProtocolType.Udp);
            Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                FreeResources();
            }
        }

        public void DropMulticastGroup(IPAddress multicastAddress)
        {
            if (_cleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (multicastAddress == null)
            {
                throw new ArgumentNullException("multicastAddress");
            }
            if (multicastAddress.AddressFamily != _family)
            {
                throw new ArgumentException("Invalid multicast family.", "multicastAddress");
            }
            if (_family == AddressFamily.InterNetwork)
            {
                MulticastOption optionValue = new MulticastOption(multicastAddress);
                Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, optionValue);
            }
            else
            {
                IPv6MulticastOption option2 = new IPv6MulticastOption(multicastAddress);
                Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership, option2);
            }
        }

        public void DropMulticastGroup(IPAddress multicastAddress, int index)
        {
            if (_cleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (multicastAddress == null)
            {
                throw new ArgumentNullException("multicastAddress");
            }
            if (index < 0)
            {
                throw new ArgumentException("Value cannot be negative.", "index");
            }
            if (_family != AddressFamily.InterNetworkV6)
            {
                throw new SocketException((int) SocketError.OperationNotSupported);
            }
            IPv6MulticastOption optionValue = new IPv6MulticastOption(multicastAddress, index);
            Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership, optionValue);
        }

        private void FreeResources()
        {
            if (!_cleanedUp)
            {
                Socket client = Client;
                if (client != null)
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    Client = null;
                }
                _cleanedUp = true;
            }
        }

        public void JoinMulticastGroup(IPAddress multicastAddress)
        {
            if (_cleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (multicastAddress == null)
            {
                throw new ArgumentNullException("multicastAddress");
            }
            if (multicastAddress.AddressFamily != _family)
            {
                throw new ArgumentException("Invalid multicast family.", "multicastAddress");
            }
            if (_family == AddressFamily.InterNetwork)
            {
                MulticastOption optionValue = new MulticastOption(multicastAddress);
                Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
            }
            else
            {
                IPv6MulticastOption option2 = new IPv6MulticastOption(multicastAddress);
                Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, option2);
            }
        }

        public void JoinMulticastGroup(int index, IPAddress multicastAddress)
        {
            if (_cleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (multicastAddress == null)
            {
                throw new ArgumentNullException("multicastAddress");
            }
            if (index < 0)
            {
                throw new ArgumentException("Value cannot be negative.", "index");
            }
            if (_family != AddressFamily.InterNetworkV6)
            {
                throw new SocketException((int) SocketError.OperationNotSupported);
            }
            IPv6MulticastOption optionValue = new IPv6MulticastOption(multicastAddress, index);
            Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, optionValue);
        }

        public void JoinMulticastGroup(IPAddress multicastAddress, int timeToLive)
        {
            if (_cleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (multicastAddress == null)
            {
                throw new ArgumentNullException("multicastAddress");
            }
            JoinMulticastGroup(multicastAddress);
            Client.SetSocketOption((_family == AddressFamily.InterNetwork) ? SocketOptionLevel.IP : SocketOptionLevel.IPv6, SocketOptionName.MulticastTimeToLive, timeToLive);
        }

        public void JoinMulticastGroup(IPAddress multicastAddress, IPAddress localAddress)
        {
            if (_cleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (_family != AddressFamily.InterNetwork)
            {
                throw new SocketException((int) SocketError.OperationNotSupported);
            }
            MulticastOption optionValue = new MulticastOption(multicastAddress, localAddress);
            Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
        }

        public int Receive(byte[] buffer, int offset, int size)
        {
            if (_cleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            EndPoint any = _family == AddressFamily.InterNetwork ? new IPEndPoint(IPAddress.Any, 0) : new IPEndPoint(IPAddress.IPv6Any, 0);
            return Client.ReceiveFrom(buffer, offset, size, SocketFlags.None, ref any);
        }

        public int Send(byte[] datagram, int bytes)
        {
            if (_cleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (datagram == null)
            {
                throw new ArgumentNullException("datagram");
            }
            if (!_active)
            {
                throw new InvalidOperationException("Not connected.");
            }
            return Client.Send(datagram, 0, bytes, SocketFlags.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "endPoint")]
        public int Send(byte[] datagram, int bytes, IPEndPoint endPoint)
        {
            if (_cleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (datagram == null)
            {
                throw new ArgumentNullException("datagram");
            }
            if (_active && (endPoint != null))
            {
                throw new InvalidOperationException("Udp connected");
            }
            if (endPoint == null)
            {
                return Client.Send(datagram, 0, bytes, SocketFlags.None);
            }
            return Client.SendTo(datagram, 0, bytes, SocketFlags.None, endPoint);
        }

        public int Send(byte[] datagram, int bytes, string hostName, int port)
        {
            if (_cleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (datagram == null)
            {
                throw new ArgumentNullException("datagram");
            }
            if (_active && ((hostName != null) || (port != 0)))
            {
                throw new InvalidOperationException("Udp Connected");
            }
            if ((hostName == null) || (port == 0))
            {
                return Client.Send(datagram, 0, bytes, SocketFlags.None);
            }
            IPAddress[] hostAddresses = Dns.GetHostAddresses(hostName);
            int index = 0;
            while ((index < hostAddresses.Length) && (hostAddresses[index].AddressFamily != _family))
            {
                index++;
            }
            if ((hostAddresses.Length == 0) || (index == hostAddresses.Length))
            {
                throw new ArgumentException("Invalid AddressList.", "hostName");
            }
            IPEndPoint remoteEP = new IPEndPoint(hostAddresses[index], port);
            return Client.SendTo(datagram, 0, bytes, SocketFlags.None, remoteEP);
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

        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Dont")]
        public bool DontFragment
        {
            get { return _clientSocket.DontFragment; }
            set { _clientSocket.DontFragment = value; }
        }

        public bool EnableBroadcast
        {
            get { return _clientSocket.EnableBroadcast; }
            set { _clientSocket.EnableBroadcast = value; }
        }

        public bool ExclusiveAddressUse
        {
            get { return _clientSocket.ExclusiveAddressUse; }
            set { _clientSocket.ExclusiveAddressUse = value; }
        }

        public bool MulticastLoopback
        {
            get { return _clientSocket.MulticastLoopback; }
            set { _clientSocket.MulticastLoopback = value; }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ttl")]
        public short Ttl
        {
            get { return _clientSocket.Ttl; }
            set { _clientSocket.Ttl = value; }
        }

    }

    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "EndPoint"), Serializable]
    public class UdpEndPoint : EndPoint
    {
        // Fields
        // ReSharper disable InconsistentNaming
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal static IPEndPoint Any = new IPEndPoint(IPAddress.Any, 0);
        internal const int AnyPort = 0;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal static IPEndPoint IPv6Any = new IPEndPoint(IPAddress.IPv6Any, 0);
        // ReSharper restore InconsistentNaming
        private IPAddress _address;
        private int _port;
        public const int MaxPort = 0xffff;
        public const int MinPort = 0;

        // Methods
        public UdpEndPoint(long address, int port)
        {
            _port = port;
            _address = new IPAddress(address);
        }

        public UdpEndPoint(IPAddress address, int port)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            _port = port;
            _address = address;
        }

        public override EndPoint Create(SocketAddress socketAddress)
        {
            if (socketAddress.Family != AddressFamily)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "InvalidAddressFamily {0} {1} {2}.", socketAddress.Family, GetType().FullName, AddressFamily), "socketAddress");
            }
            if (socketAddress.Size < 8)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "InvalidSocketAddressSize {0} {1} {2}.", socketAddress.Family, GetType().FullName, AddressFamily), "socketAddress");
            }
            if (AddressFamily == AddressFamily.InterNetworkV6)
            {
                byte[] address = new byte[0x10];
                for (int i = 0; i < address.Length; i++)
                {
                    address[i] = socketAddress[i + 8];
                }
                int num2 = ((socketAddress[2] << 8) & 0xff00) | socketAddress[3];
                long scopeid = (((socketAddress[0x1b] << 0x18) + (socketAddress[0x1a] << 0x10)) + (socketAddress[0x19] << 8)) + socketAddress[0x18];
                return new IPEndPoint(new IPAddress(address, scopeid), num2);
            }
            int port = ((socketAddress[2] << 8) & 0xff00) | socketAddress[3];
            return new IPEndPoint(((((socketAddress[4] & 0xff) | ((socketAddress[5] << 8) & 0xff00)) | ((socketAddress[6] << 0x10) & 0xff0000)) | (socketAddress[7] << 0x18)) & 0xffffffffL, port);
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily"), SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        public override bool Equals(object comparand)
        {
            return ((comparand is IPEndPoint) && (((IPEndPoint)comparand).Address.Equals(_address) && (((IPEndPoint)comparand).Port == _port)));
        }

        public override int GetHashCode()
        {
            return (_address.GetHashCode() ^ _port);
        }

        public override SocketAddress Serialize()
        {
            if (_address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                SocketAddress address = new SocketAddress(AddressFamily, 0x1c);
                int port = Port;
                address[2] = (byte)(port >> 8);
                address[3] = (byte)port;
                address[4] = 0;
                address[5] = 0;
                address[6] = 0;
                address[7] = 0;
                long scopeId = Address.ScopeId;
                address[0x18] = (byte)scopeId;
                address[0x19] = (byte)(scopeId >> 8);
                address[0x1a] = (byte)(scopeId >> 0x10);
                address[0x1b] = (byte)(scopeId >> 0x18);
                byte[] addressBytes = Address.GetAddressBytes();
                for (int i = 0; i < addressBytes.Length; i++)
                {
                    address[8 + i] = addressBytes[i];
                }
                return address;
            }
            SocketAddress address2 = new SocketAddress(_address.AddressFamily, 0x10);
            address2[2] = (byte)(Port >> 8);
            address2[3] = (byte)Port;
            #pragma warning disable 612,618
            address2[4] = (byte) Address.Address;
            address2[5] = (byte)(Address.Address >> 8);
            address2[6] = (byte)(Address.Address >> 0x10);
            address2[7] = (byte)(Address.Address >> 0x18);
            #pragma warning restore 612,618
            return address2;
        }

        public override string ToString()
        {
            return (Address + ":" + Port.ToString(NumberFormatInfo.InvariantInfo));
        }

        // Properties
        public IPAddress Address
        {
            get { return _address; }
            set { _address = value; }
        }

        public override AddressFamily AddressFamily
        {
            get { return _address.AddressFamily; }
        }

        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

    }
}
