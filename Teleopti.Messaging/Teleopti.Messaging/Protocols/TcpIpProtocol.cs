using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Teleopti.Interfaces.MessageBroker.Coders;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Coders;
using Teleopti.Messaging.Exceptions;
using Teleopti.Messaging.Server;
using log4net;

namespace Teleopti.Messaging.Protocols
{
    /// <summary>
    /// Implements the TcpIpProtocol for sending message information.
    /// </summary>
    public class TcpIpProtocol : Protocol
    {
		private static ILog Logger = LogManager.GetLogger(typeof(TcpIpProtocol));
        private CustomTcpListener _tcpListener;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpIpProtocol"/> class.
        /// </summary>
        /// <param name="socketInformation">The socket information.</param>
        public TcpIpProtocol(ISocketInfo socketInformation) : base(socketInformation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpIpProtocol"/> class.
        /// </summary>
        /// <param name="brokerService">The broker service.</param>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="timeToLive">The time to live.</param>
        public TcpIpProtocol(IBrokerService brokerService, string address, int port, int timeToLive) : base(brokerService, address, port, timeToLive)
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
                using (TcpSender sender = new TcpSender())
                {
                    try
                    {
                        IPAddress ipAddress = IPAddress.Parse(Address);
                    	sender.Connect(new IPEndPoint(ipAddress, Port));

                        NetworkStream networkStream = sender.GetStream();
                        networkStream.Write(value, 0, value.Length);
                        networkStream.Close();
                        sender.Close();
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
                        Logger.Warn("Send package warning.", clientNotConnected);
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
                _tcpListener = new CustomTcpListener(SocketInformation.Port);
                _tcpListener.Start();
            }
            catch (SocketException socketException)
            {
                Logger.Error("Start listener error.", socketException);
                throw;
            }
        }

        private ManualResetEvent _acceptReset = new ManualResetEvent(false);

        /// <summary>
        /// Read a byte stream from the socket.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override void ReadByteStream()
        {
            try
            {
                _acceptReset.Reset();
                _tcpListener.BeginAcceptTcpSender(OnEndAcceptTcpSender);
                _acceptReset.WaitOne();
            }
            catch (Exception exc)
            {
                Logger.Error("Read byte stream error.", exc);
            }
        }

        private class NetworkAsyncState
        {
            public NetworkAsyncState()
            {
                RecieveBuffer = new byte[Consts.MaxWireLength];
                WaitHandle = new ManualResetEvent(false);
            }
            public byte[] RecieveBuffer { get; private set; }
            public NetworkStream NetworkStream { get; set; }
            public ManualResetEvent WaitHandle { get; private set; }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void OnEndAcceptTcpSender(IAsyncResult ar)
        {
            try
            {
                if (!_acceptReset.SafeWaitHandle.IsClosed)
                    _acceptReset.Set(); //Allow new sockets to be accepted when we reach this point

                ITcpSender sender = _tcpListener.EndAcceptTcpSender(ar);
                using (NetworkStream networkStream = sender.GetStream())
                {
                    // Check to see if this NetworkStream is readable.
                    if (networkStream.CanRead)
                    {
                        NetworkAsyncState asyncState = new NetworkAsyncState {NetworkStream = networkStream};
                        networkStream.BeginRead(asyncState.RecieveBuffer, 0, asyncState.RecieveBuffer.Length,
                                                networkStreamReadCallback, asyncState);
                        asyncState.WaitHandle.WaitOne();
                    }
                    //networkStream.Close();
                    //sender.Close();
                }
                sender.Dispose();
            }
            catch (Exception exc)
            {
                Logger.Error("Accept tcp sender error.", exc);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void networkStreamReadCallback(IAsyncResult ar)
        {
            try
            {
                NetworkAsyncState networkAsyncState = (NetworkAsyncState) ar.AsyncState;
                networkAsyncState.NetworkStream.EndRead(ar);

                if (networkAsyncState.NetworkStream.DataAvailable)
                {
                    networkAsyncState.NetworkStream.BeginRead(networkAsyncState.RecieveBuffer, 0,
                                                              networkAsyncState.RecieveBuffer.Length,
                                                              networkStreamReadCallback, networkAsyncState);
                }
                else
                {
                    if (!networkAsyncState.WaitHandle.SafeWaitHandle.IsClosed)
                        networkAsyncState.WaitHandle.Set();
                    MessagingHandlerPool.QueueUserWorkItem(DecodeEventMessage, networkAsyncState.RecieveBuffer);
                }
            }
            catch (Exception exc)
            {
                Logger.Error("Read callback error.", exc);
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
                if (_acceptReset!=null)
                {
                    _acceptReset.Close();
                }
                if (_tcpListener != null)
                {
                    _tcpListener.Stop();
                    _tcpListener.Dispose();
                }
            }
            catch (SocketException)
            {
            }
            finally
            {
                _tcpListener = null;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(true);
            if (isDisposing)
            {
                if (_acceptReset != null)
                {
                    _acceptReset.Close();
                }
                if (_tcpListener != null)
                {
                    try
                    {
                        _tcpListener.Stop();
                        _tcpListener.Dispose();
                    }
                    catch (SocketException)
                    {
                    }
                    finally
                    {
                        _tcpListener = null;
                    }
                }
            }
        }
    }
}
