using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Sockets;
using System.Threading;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Logging;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.Events;

namespace Teleopti.Messaging.Protocols
{
    /// <summary>
    /// The client.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 12/05/2010
    /// </remarks>
    public class ClientImplementation : IDisposable
    {
        private readonly Queue<MessageQueueItem> _queue = new Queue<MessageQueueItem>();
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);
        private readonly Semaphore _workWaiting = new Semaphore(0, int.MaxValue);
        private UnhandledExceptionEventHandler _unhandledExceptionEventHandler;
        private ICustomTcpListener _tcpListener;
        private Thread _messageLoopThread;
        private int _clientThrottle;
        private int _port;
        private string _address;
        private IBrokerService _brokerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="brokerService">The broker service.</param>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="clientThrottle">The client throttle.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/05/2010
        /// </remarks>
        public ClientImplementation(IBrokerService brokerService, string address, int port, int clientThrottle)
        {
            _brokerService = brokerService;
            _address = address;
            _clientThrottle = clientThrottle;
            _port = port;
            Initialize();
        }

        /// <summary>
        /// Initialises this instance.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/05/2010
        /// </remarks>
        public void Initialize()
        {
            _messageLoopThread = new Thread(Receive);
            _messageLoopThread.Name = "Message Loop Thread";
            _messageLoopThread.IsBackground = true;
            _messageLoopThread.Start();
        }

        /// <summary>
        /// Occurs when [unhandled exception event handler].
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/05/2010
        /// </remarks>
        public event UnhandledExceptionEventHandler UnhandledExceptionEventHandler
        {
            add { _unhandledExceptionEventHandler += value; }
            remove { _unhandledExceptionEventHandler -= value; }
        }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>The address.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/05/2010
        /// </remarks>
        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/05/2010
        /// </remarks>
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        /// <summary>
        /// Start receiving but starting the 
        /// eternal receiving loop.
        /// </summary>
        private void Receive()
        {
            try
            {
                _tcpListener = new CustomTcpListener(_port);
                _tcpListener.Start();
                while (!_resetEvent.WaitOne(_clientThrottle, false))
                {
                    ReadByteStream();
                }
            }
            catch (ThreadInterruptedException)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Client thread interrupted!");
            }
            catch (ThreadAbortException)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Client thread aborted!");
            }
            catch (ObjectDisposedException)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Client thread disposed!");
            }
        }

        /// <summary>
        /// Read a byte stream from the socket.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void ReadByteStream()
        {
            try
            {
                using (ITcpSender sender = _tcpListener.AcceptTcpSender())
                {
                    using (NetworkStream networkStream = sender.GetStream())
                    {
                        _workWaiting.WaitOne();
                        MessageQueueItem item = GetItem();
                        if (networkStream.CanWrite && item != null)
                            networkStream.Write(item.Value, 0, item.Value.Length);
                        networkStream.Close();
                        sender.Close();
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
        /// Deregisters the subscriber.
        /// </summary>
        protected void DeregisterSubscriber()
        {
            _brokerService.UnregisterSubscriber(Address, Port);
            Logger.Instance.WriteLine(EventLogEntryType.Error, GetType(), "Client with Address {0} and port {1} is disconnected.");
        }


        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/05/2010
        /// </remarks>
        private MessageQueueItem GetItem()
        {
            MessageQueueItem item = null;
            lock (_queue)
            {
                if (_queue.Count > 0)
                    item = _queue.Dequeue();
            }
            return item;
        }

        /// <summary>
        /// Queues the item.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/05/2010
        /// </remarks>
        public void QueueItem(byte[] value)
        {
            MessageQueueItem item = new MessageQueueItem(_address, _port, value);
            lock (_queue)
                _queue.Enqueue(item);
            _workWaiting.Release();
        }

        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {

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

                if (_workWaiting != null)
                    _workWaiting.Close();
                
                if (_resetEvent != null)
                    _resetEvent.Close();

                try
                {
                    _messageLoopThread.Interrupt();
                    _messageLoopThread = null;
                }
                catch (ThreadInterruptedException)
                {
                }

            }
        }
    }


   
}