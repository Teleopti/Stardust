// ReSharper disable FieldCanBeMadeReadOnly.Local
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.Events;
using Teleopti.Messaging.Server;

namespace Teleopti.Messaging.Protocols
{
    /// <summary>
    /// The client initiated tcp/ip protocol.
    /// </summary>
    public class ClientTcpIpProtocol : Protocol
    {
        private int _startPort;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientTcpIpProtocol"/> class.
        /// </summary>
        /// <param name="socketInformation">The socket information.</param>
        public ClientTcpIpProtocol(ISocketInfo socketInformation) : base(socketInformation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpIpProtocol"/> class.
        /// </summary>
        /// <param name="brokerService">The broker service.</param>
        /// <param name="startPort">The start port.</param>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="timeToLive">The time to live.</param>
        public ClientTcpIpProtocol(IBrokerService brokerService, int startPort, string address, int port, int timeToLive) : base(brokerService, address, port, timeToLive)
        {
            _startPort = startPort;
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="value">The package.</param>
        public override void SendPackage(byte[] value)
        {
            ClientTcpIpManager.GetInstance(_startPort).SendPackage(Address, Port, value);
        }

        /// <summary>
        /// Starts the listener.
        /// </summary>
        protected override void StartListener()
        {
        }

        /// <summary>
        /// Reads the byte stream.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override void ReadByteStream()
        {
            try
            {
                using (TcpSender sender = new TcpSender())
                {
                    IPAddress ipAddress = IPAddress.Parse(SocketUtility.IsAddress(SocketInformation.Address) ? SocketInformation.Address : SocketUtility.GetIPAddressByHostName(SocketInformation.Address));
                    if (ipAddress != null)
                        sender.Connect(new IPEndPoint(ipAddress, SocketInformation.Port));
                    if (sender.Connected)
                    {
                        using (NetworkStream networkStream = sender.GetStream())
                        {
                            // Check to see if this NetworkStream is readable.
                            if (networkStream.CanRead)
                            {
                                do
                                {
                                    byte[] receiveBuffer = new byte[Consts.MaxWireLength];
                                    // Incoming message may be larger than the buffer size.
                                    networkStream.Read(receiveBuffer, 0, receiveBuffer.Length);
                                    MessagingHandlerPool.QueueUserWorkItem(DecodeEventMessage, receiveBuffer);
                                }
                                while (networkStream.DataAvailable);
                            }
                        }
                    }
                }
            }
            catch (SocketException socketException)
            {
                Logger.Instance.WriteLine(EventLogEntryType.Error, GetType(), String.Format(CultureInfo.InvariantCulture, "ErrorCode: {0}, Exception Description: {1}.", socketException.ErrorCode, socketException));
            }
            catch (Exception exc)
            {
                Logger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.ToString());
            }
        }


        /// <summary>
        /// Stops the subscribing.
        /// </summary>
        public override void StopSubscribing()
        {
            try
            {

                if (SocketThreadPool != null)
                    SocketThreadPool.Dispose();

                if (ResetEvent != null)
                    ResetEvent.Close();

            }
            catch (SocketException)
            {
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(true);
            if (isDisposing)
            {
                StopSubscribing();
            }
        }

    }
}
// ReSharper restore FieldCanBeMadeReadOnly.Local