using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Teleopti.Interfaces.MessageBroker.Coders;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Coders;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Messaging.Protocols
{
    /// <summary>
    /// Implements the UdpProtocol for sending message information.
    /// </summary>
    public class UdpProtocol : Protocol
    {
        private CustomUdpListener _udpListener;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpIpProtocol"/> class.
        /// </summary>
        /// <param name="socketInformation">The socket information.</param>
        public UdpProtocol(ISocketInfo socketInformation) : base(socketInformation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpIpProtocol"/> class.
        /// </summary>
        /// <param name="brokerService">The broker service.</param>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="timeToLive">The time to live.</param>
        public UdpProtocol(IBrokerService brokerService, string address, int port, int timeToLive) : base(brokerService, address, port, timeToLive)
        {
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 15/05/2010
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override void SendPackage(IEventMessage eventMessage)
        {
            try
            {
                IEventMessageEncoder encoder = new EventMessageEncoder();
                byte[] value = encoder.Encode(eventMessage);
                using (Socket sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    try
                    {
                        sendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                        IPAddress ipAddress = IPAddress.Parse(Address);
                        if (ipAddress != null)
                        {
                            EndPoint sendEndPoint = new IPEndPoint(ipAddress, Port);
                            sendSocket.SendTo(value, 0, value.Length, SocketFlags.None, sendEndPoint);
                        }
                        else
                        {
                            throw new ArgumentException("IP Address string could not be parsed.");
                        }
                        sendSocket.Close();
                    }
                    catch (ArgumentException exc)
                    {
                        DeregisterSubscriber();
                        BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.ToString());
                    }
                    catch (SocketException socketException)
                    {
                        ClientNotConnectedException clientNotConnected = new ClientNotConnectedException(String.Format(CultureInfo.InvariantCulture, "Client {0} on port {1} is not connected. WinSocket Error: {2}, Inner Exception: {3}.", Address, Port, socketException.ErrorCode, socketException.Message), socketException);
                        DeregisterSubscriber();
                        BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), clientNotConnected.Message);
                    }
                    catch (NullReferenceException nullReferenceException)
                    {
                        DeregisterSubscriber();
                        BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), nullReferenceException.ToString());
                    }
                    catch (Exception exc)
                    {
                        BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.ToString());
                    }
                }
            }
            catch (Exception exception)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exception.ToString());
            }
        }

        /// <summary>
        /// Starts the listner.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2009-03-28
        /// </remarks>
        protected override void StartListener()
        {
            try
            {
                _udpListener = new CustomUdpListener(SocketInformation.Port);
                _udpListener.Start();
            }
            catch (SocketException socketException)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, typeof(SocketException), socketException.ToString());
                throw;
            }
        }

        /// <summary>
        /// Read a byte stream from the socket.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override void ReadByteStream()
        {
            try
            {
                using (IUdpSender sender = _udpListener.AcceptUdpSender())
                {
                    do
                    {
                        byte[] receiveBuffer = new byte[Consts.MaxWireLength];
                        try
                        {
                            sender.Receive(receiveBuffer, 0, receiveBuffer.Length);
                            MessagingHandlerPool.QueueUserWorkItem(DecodeEventMessage, receiveBuffer);
                        }
                        catch (SocketException socketException)
                        {
                            BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), String.Format(CultureInfo.InvariantCulture, "ErrorCode: {0}, Exception Description: {1}.", socketException.ErrorCode, socketException));
                        }
                    }
                    while (sender.Available > 0);
                }
            }
            catch (SocketException socketException)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), String.Format(CultureInfo.InvariantCulture, "ErrorCode: {0}, Exception Description: {1}.", socketException.ErrorCode, socketException));
            }
            catch (Exception exc)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.ToString());
            }
        }

        /// <summary>
        /// Stops the subscribing.
        /// </summary>
        public override void StopSubscribing()
        {
            try
            {
                if (ResetEvent != null)
                    ResetEvent.Close();

                if (_udpListener != null)
                {
                    _udpListener.Stop();
                    _udpListener.Dispose();
                }
            }
            catch (SocketException)
            {
            }
            finally
            {
                _udpListener = null;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(true);
            if (isDisposing)
            {
                if (_udpListener != null)
                {
                    try
                    {
                        _udpListener.Stop();
                        _udpListener.Dispose();
                    }
                    catch (SocketException)
                    {
                    }
                    finally
                    {
                        _udpListener = null;
                    }
                }
            }
        }
    }
}
