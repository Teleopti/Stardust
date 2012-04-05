using System;
using System.Diagnostics;
using System.Security;
using System.Threading;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging;

namespace Teleopti.Messaging.Client
{
	public class Subscriber : ISubscriber
	{
		private const string TeleoptiDeserialisationThread = " Teleopti Deserialisation Thread";
		private CustomThreadPool _customThreadPool;
		private bool _isStarted;
		private Thread _messageLoopThread;
		private readonly ISocketInfo _socketInformation;
		private IProtocol _protocol;

		public Subscriber(ISocketInfo socketInformation, IProtocol protocol)
		{
			_socketInformation = socketInformation;
			_protocol = protocol;
		}

		/// <summary>
		/// Gets the port.
		/// </summary>
		/// <value>The port.</value>
		public int Port
		{
			get { return _socketInformation.Port; }
		}

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

		/// <summary>
		/// Receive Event Messages
		/// </summary>
		public event EventHandler<EventMessageArgs> EventMessageHandler
		{
			add
			{
				if (_protocol != null)
					_protocol.EventMessageHandler += value;
			}
			remove
			{
				if (_protocol != null)
					_protocol.EventMessageHandler -= value;
			}
		}

		/// <summary>
		/// Subscribe to unhandled exceptions on background threads.
		/// </summary>
		public event EventHandler<UnhandledExceptionEventArgs> UnhandledExceptionHandler
		{
			add
			{
				if (_protocol!=null)
					_protocol.UnhandledExceptionHandler += value;
			}
			remove
			{
				if (_protocol != null)
					_protocol.UnhandledExceptionHandler -= value;
			}
		}

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
				{
					_customThreadPool.Dispose();
				}

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
				catch (SecurityException)
				{
				}
			}
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				StopSubscribing();

				_messageLoopThread = null;
				_protocol = null;
				_customThreadPool = null;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}