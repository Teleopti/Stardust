// ReSharper disable FieldCanBeMadeReadOnly.Local
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.Events;
using Teleopti.Messaging.Server;

namespace Teleopti.Messaging.Protocols
{
    /// <summary>
    /// The client initiated tcp/ip protocol.
    /// </summary>
    public class ClientUdpProtocol : Protocol
    {
        private int _startPort;
        private IMessageBroker _messageBroker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientProtocol"/> class.
        /// </summary>
        /// <param name="messageBroker">The message broker.</param>
        /// <param name="socketInformation">The socket information.</param>
        public ClientUdpProtocol(IMessageBroker messageBroker, ISocketInfo socketInformation) : base(socketInformation)
        {
            _messageBroker = messageBroker;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientProtocol"/> class.
        /// </summary>
        /// <param name="brokerService">The broker service.</param>
        /// <param name="startPort">The start port.</param>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="timeToLive">The time to live.</param>
        public ClientUdpProtocol(IBrokerService brokerService, int startPort, string address, int port, int timeToLive) : base(brokerService, address, port, timeToLive)
        {
            _startPort = startPort;
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="value">The package.</param>
        public override void SendPackage(byte[] value)
        {
            ClientUdpManager.GetInstance(BrokerService, _startPort, ClientThrottle).QueueMessage(Address, Port, value);
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
                    try
                    {
                        if (ipAddress != null)
                            sender.Connect(new IPEndPoint(ipAddress, SocketInformation.Port));
                    }
                    catch (SocketException)
                    {
                    }
                    if (!sender.Connected)
                    {
                        int oldPort = SocketInformation.Port;
                        SocketInformation.Port = _messageBroker.RetrieveNewPort();
                        Logger.Instance.WriteLine(EventLogEntryType.Information, GetType(), String.Format(CultureInfo.InvariantCulture, "New port for subscriber : {0}, Port: {1}, Old Port: {2}.", SocketInformation.Address, SocketInformation.Port, oldPort));
                    }
                    else if (sender.Connected)
                    {
                        using (NetworkStream networkStream = sender.GetStream())
                        {
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
                                networkStream.Close();
                                sender.Close();
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

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="isDisposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 29/04/2010
        /// </remarks>
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