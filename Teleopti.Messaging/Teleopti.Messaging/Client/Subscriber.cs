using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging;

namespace Teleopti.Messaging.Client
{
    [Serializable]
    public class Subscriber : ISubscriber
    {

        #region Fields

        private const string TeleoptiDeserialisationThread = " Teleopti Deserialisation Thread";
        [NonSerialized]
        private CustomThreadPool _customThreadPool;
        [NonSerialized]
        private bool _isStarted;
        private Thread _messageLoopThread;
        private readonly ISocketInfo _socketInformation;
        private readonly IProtocol _protocol;

        #endregion

        #region Constructor

        public Subscriber(ISocketInfo socketInformation, IProtocol protocol)
        {
            _socketInformation = socketInformation;
            _protocol = protocol;
        }

        protected Subscriber(SerializationInfo info, StreamingContext context)
        {
            _socketInformation = (ISocketInfo)info.GetValue("SocketInformation", typeof(SocketInformation));
        }

        #endregion

        #region Intialise Method

        /// <summary>
        /// Gets the port.
        /// </summary>
        /// <value>The port.</value>
        public int Port
        {
            get { return _socketInformation.Port; }
        }

        #endregion

        #region Private Receive Implementation

        /// <summary>
        /// Start receiving but starting the 
        /// eternal receiving loop.
        /// </summary>
        private void Receive()
        {
            try
            {
                while (!_protocol.ResetEvent.WaitOne(_protocol.ClientThrottle, false))
                {
                    if (_socketInformation != null)
                    {
                        _protocol.ReadByteStream();
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Subscriber thread interrupted!");
            }
            catch (ThreadAbortException)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Subscriber thread aborted!");
            }
            catch (ObjectDisposedException)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Subscriber thread disposed!");
            }
            catch (NullReferenceException)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Warning, GetType(), "Protocol disposed!");
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Receive Event Messages
        /// </summary>
        public event EventHandler<EventMessageArgs> EventMessageHandler
        {
            add { _protocol.EventMessageHandler += value; }
            remove { _protocol.EventMessageHandler -= value; }
        }

        /// <summary>
        /// Subscribe to unhandled exceptions on background threads.
        /// </summary>
        public event EventHandler<UnhandledExceptionEventArgs> UnhandledExceptionHandler
        {
            add { _protocol.UnhandledExceptionHandler += value; }
            remove { _protocol.UnhandledExceptionHandler -= value; }
        }

        #endregion

        #region Public Methods to implement ISubscriber

        /// <summary>
        /// Starts the subscriber, serialisation threads.
        /// A low number of threads would be sufficient, e.g. 1 - 3 threads.
        /// </summary>
        /// <param name="threads">The number of threads you want handling incomming messages</param>
        public void StartSubscribing(int threads)
        {
            if (!_isStarted)
            {
                _customThreadPool = new CustomThreadPool(threads, TeleoptiDeserialisationThread);
                _customThreadPool.UnhandledException += _protocol.OnUnhandledException;
                _protocol.Initialise(_customThreadPool);
                // Call receive to start to subscribe to messages
                _messageLoopThread = new Thread(Receive);
                _messageLoopThread.Name = "Message Loop Thread";
                _messageLoopThread.IsBackground = true;
                _messageLoopThread.Start();
            }
        }

        /// <summary>
        /// Stop subscribing to event messages.
        /// </summary>
        public void StopSubscribing()
        {
            if (_socketInformation != null)
            {
                _isStarted = false;

                if (_customThreadPool != null)
                    _customThreadPool.Dispose();

                if (_protocol != null)
                {
                    _protocol.StopSubscribing();
                    _protocol.Dispose();
                }

                try
                {
                    if (_messageLoopThread != null)
                        _messageLoopThread.Interrupt();
                }
                catch (ThreadInterruptedException)
                {
                }
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="isDisposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
                StopSubscribing();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Serialisation Method

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("SocketInformation", _socketInformation, _socketInformation.GetType());
        }

        #endregion

    }
}