using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Exceptions;
using log4net;

namespace Teleopti.Messaging.SignalR
{
	public class SignalSenderBase
	{
		protected string _serverUrl;
		protected ISignalWrapper _wrapper;

		[CLSCompliant(false)]
		protected ILog Logger;

		public SignalSenderBase(string serverUrl)
		{
			_serverUrl = serverUrl;

			ServicePointManager.ServerCertificateValidationCallback = ignoreInvalidCertificate;
			ServicePointManager.DefaultConnectionLimit = 50;

			TaskScheduler.UnobservedTaskException += taskSchedulerOnUnobservedTaskException;
		}

		public bool IsAlive
		{
			get { return _wrapper != null && _wrapper.IsInitialized(); }
		}

		protected static bool ignoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
		{
			return true;
		}

		protected void taskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			if (e.Observed) return;
			Logger.Error("An error occured, please review the error and take actions necessary.", e.Exception);
			e.SetObserved();
		}

		public void StartBrokerService()
		{
			try
			{
				var connection = MakeHubConnection();
				var proxy = connection.CreateHubProxy("MessageBrokerHub");

				_wrapper = _wrapper ?? new SignalWrapper(proxy, connection, Logger);
				_wrapper.StartHub();
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

		[CLSCompliant(false)]
		protected virtual IHubConnectionWrapper MakeHubConnection()
		{
			return new HubConnectionWrapper(new HubConnection(_serverUrl));
		}
	}

	public class AsyncSignalSender : SignalSenderBase, IAsyncMessageSender
	{
		private readonly BlockingCollection<Tuple<DateTime, Notification>> _notificationQueue = new BlockingCollection<Tuple<DateTime, Notification>>();
		private readonly CancellationTokenSource cancelToken = new CancellationTokenSource();
		private Thread workerThread;

		public AsyncSignalSender(string serverUrl)
			: base(serverUrl)
		{
			// ReSharper disable DoNotCallOverridableMethodsInConstructor
			Logger = MakeLogger();
			StartWorkerThread();
			// ReSharper restore DoNotCallOverridableMethodsInConstructor
		}

		public void SendNotificationAsync(Notification notification)
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

		protected virtual void ProcessTheQueue()
		{
			var notifications =
				_notificationQueue.GetConsumingEnumerable(cancelToken.Token)
				                  .Take(Math.Max(1, Math.Min(_notificationQueue.Count, 20)))
				                  .ToArray()
				                  .Where(t => t.Item1 > CurrentUtcTime().AddMinutes(-2))
				                  .Select(t => t.Item2)
				                  .ToArray();

			if (!notifications.Any()) return;
			trySend(notifications);
		}

		private void trySend(IEnumerable<Notification> notifications)
		{
			try
			{
				var task = _wrapper.NotifyClients(notifications);
				task.Wait(1000, cancelToken.Token);
			}
			catch (AggregateException e)
			{
				Logger.Error("Could not send notifications, ", e);
			}
		}

		public void Dispose()
		{
			cancelToken.Cancel();
			workerThread.Join();
			_wrapper.StopHub();
			_wrapper = null;
		}


		[CLSCompliant(false)]
		protected virtual DateTime CurrentUtcTime()
		{
			return DateTime.UtcNow;
		}

		[CLSCompliant(false)]
		protected virtual void StartWorkerThread()
		{
			workerThread = new Thread(processQueue) { IsBackground = true };
			workerThread.Start();
		}

		[CLSCompliant(false)]
		protected virtual ILog MakeLogger()
		{
			return LogManager.GetLogger(typeof(AsyncSignalSender));
		}
	}
}