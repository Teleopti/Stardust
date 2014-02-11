﻿using System;
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
	public class AsyncSignalSender : IAsyncMessageSender
	{
		private readonly BlockingCollection<Tuple<DateTime, Notification>> _notificationQueue = new BlockingCollection<Tuple<DateTime, Notification>>();
		private readonly CancellationTokenSource cancelToken = new CancellationTokenSource();
		private Thread workerThread;

		protected string ServerUrl;
		protected ISignalWrapper Wrapper;
		protected ILog Logger;

		public AsyncSignalSender(string serverUrl)
		{
			ServerUrl = serverUrl;

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
			e.SetObserved();
		}

		public void StartBrokerService()
		{
			try
			{
				var connection = MakeHubConnection();
				var proxy = connection.CreateHubProxy("MessageBrokerHub");

				Wrapper = Wrapper ?? new SignalWrapper(proxy, connection, Logger);
				Wrapper.StartHub();
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
			get { return Wrapper != null && Wrapper.IsInitialized(); }
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
			Wrapper.NotifyClients(notifications);
		}

		public void Dispose()
		{
			cancelToken.Cancel();
			workerThread.Join();
			Wrapper.StopHub();
			Wrapper = null;
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
			return new HubConnectionWrapper(new HubConnection(ServerUrl));
		}

	}
}