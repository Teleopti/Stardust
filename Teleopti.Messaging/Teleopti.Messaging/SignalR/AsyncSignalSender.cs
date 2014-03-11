using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Exceptions;
using log4net;

namespace Teleopti.Messaging.SignalR
{
	public class AsyncSignalSender : IMessageSender
	{
		private readonly BlockingCollection<Tuple<DateTime, Notification>> _notificationQueue = new BlockingCollection<Tuple<DateTime, Notification>>();
		private readonly CancellationTokenSource cancelToken = new CancellationTokenSource();
		private Thread workerThread;

		private readonly string _serverUrl;
		private ISignalConnectionHandler _connectionHandler;
		protected ILog Logger;

		public AsyncSignalSender(string serverUrl)
		{
			_serverUrl = serverUrl;

			ServicePointManager.ServerCertificateValidationCallback = IgnoreInvalidCertificate;
			ServicePointManager.DefaultConnectionLimit = 50;

			TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
			Logger = MakeLogger();
			StartWorkerThread();
			
		}

		protected static bool IgnoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
		{
			return true;
		}

		protected void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			if (e.Observed) return;
			Logger.Debug("An unobserved task failed.", e.Exception);
			e.SetObserved();
		}
		
		public void StartBrokerService()
		{
			StartBrokerService(TimeSpan.FromSeconds(240));
		}
		
		public void StartBrokerService(TimeSpan reconnectDelay)
		{
			try
			{
				if (_connectionHandler == null)
				{
					var connection = MakeHubConnection();
					var proxy = connection.CreateHubProxy("MessageBrokerHub");
					_connectionHandler = new SignalConnectionHandler(proxy, connection, Logger, reconnectDelay);
				}
				
				_connectionHandler.StartConnection();
			}
			catch (SocketException exception)
			{
				Logger.Error("The message broker seems to be down.", exception);
			}
			catch (BrokerNotInstantiatedException exception)
			{
				Logger.Error("The message broker seems to be down.", exception);
			}
		}

		public bool IsAlive
		{
			get { return _connectionHandler != null && _connectionHandler.IsInitialized(); }
		}

		public void SendNotification(Notification notification)
		{
			_notificationQueue.Add(new Tuple<DateTime, Notification>(CurrentUtcTime(), notification));
		}

		private void processQueue(object state)
		{
			while (!cancelToken.IsCancellationRequested)
			{
				ProcessTheQueue();
			}
		}

		protected Task ProcessTheQueue()
		{
			var notifications =
				_notificationQueue.GetConsumingEnumerable(cancelToken.Token)
				                  .Take(Math.Max(1, Math.Min(_notificationQueue.Count, 20)))
				                  .ToArray()
				                  .Where(t => t.Item1 > CurrentUtcTime().AddMinutes(-2))
				                  .Select(t => t.Item2)
				                  .ToArray();

			if (!notifications.Any()) return null;
			
			return _connectionHandler.NotifyClients(notifications);
		}

		public void Dispose()
		{
			cancelToken.Cancel();
			workerThread.Join();
			_connectionHandler.CloseConnection();
			_connectionHandler = null;
		}



		protected virtual ILog MakeLogger()
		{
			return LogManager.GetLogger(typeof(AsyncSignalSender));
		}

		protected virtual DateTime CurrentUtcTime()
		{
			return DateTime.UtcNow;
		}

		protected virtual void StartWorkerThread()
		{
			workerThread = new Thread(processQueue) { IsBackground = true };
			workerThread.Start();
		}

		[CLSCompliant(false)]
		protected virtual IHubConnectionWrapper MakeHubConnection()
		{
			return new HubConnectionWrapper(new HubConnection(_serverUrl));
		}

	}
}