﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Teleopti.Interfaces.MessageBroker.Coders;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Coders;
using Teleopti.Messaging.Exceptions;
using log4net;

namespace Teleopti.Messaging.Protocols
{
    /// <summary>
    /// Implements the UdpProtocol for sending message information.
    /// </summary>
    public class UdpProtocol : Protocol
    {
		private static ILog Logger = LogManager.GetLogger(typeof(UdpProtocol));
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
                        Logger.Error("Send package error.", exc);
                    }
                    catch (SocketException socketException)
                    {
                        ClientNotConnectedException clientNotConnected = new ClientNotConnectedException(String.Format(CultureInfo.InvariantCulture, "Client {0} on port {1} is not connected. WinSocket Error: {2}, Inner Exception: {3}.", Address, Port, socketException.ErrorCode, socketException.Message), socketException);
                        DeregisterSubscriber();
                        Logger.Warn("Client not connected.", clientNotConnected);
                    }
                    catch (NullReferenceException nullReferenceException)
                    {
                        DeregisterSubscriber();
                        Logger.Error("Send package error.", nullReferenceException);
                    }
                    catch (Exception exc)
                    {
                        Logger.Error("Send package error.", exc);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error("Send package error (outer).", exception);
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
                Logger.Error("Start listener error.", socketException);
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
                            Logger.Error("Read byte stream error.", socketException);
                        }
                    }
                    while (sender.Available > 0);
                }
            }
            catch (SocketException socketException)
            {
                Logger.Error("Read byte stream error.", socketException);
            }
            catch (Exception exc)
            {
                Logger.Error("Read byte stream error.", exc);
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
