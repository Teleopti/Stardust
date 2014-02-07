using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Exceptions;
using log4net;
using Subscription = Teleopti.Interfaces.MessageBroker.Subscription;

namespace Teleopti.Messaging.SignalR
{
	public class SignalSender : IMessageSender
	{
		private readonly string _serverUrl;
		private readonly BlockingCollection<Tuple<DateTime, Notification>> _notificationQueue = new BlockingCollection<Tuple<DateTime, Notification>>();
		private readonly CancellationTokenSource cancelToken = new CancellationTokenSource();
		private ISignalWrapper _wrapper;
		private Thread workerThread;

		[CLSCompliant(false)]
		protected ILog Logger;

		public SignalSender(string serverUrl)
		{
			// ReSharper disable DoNotCallOverridableMethodsInConstructor
			Logger = MakeLogger();
			_serverUrl = serverUrl;

			ServicePointManager.ServerCertificateValidationCallback = ignoreInvalidCertificate;
			ServicePointManager.DefaultConnectionLimit = 50;

            TaskScheduler.UnobservedTaskException += taskSchedulerOnUnobservedTaskException;
			StartWorkerThread();
			// ReSharper restore DoNotCallOverridableMethodsInConstructor
		}

		private static bool ignoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
		{
			return true;
		}

		private void taskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
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

		public void SendNotification(Notification notification)
		{
			try
			{
				var task = _wrapper.NotifyClients(notification);
				task.Wait(1000);
			}
			catch (AggregateException e)
			{
				Logger.Error("Could not send notifications, ", e);
			}
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

		public bool IsAlive
		{
			get { return _wrapper != null && _wrapper.IsInitialized(); }
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
		protected virtual IHubConnectionWrapper MakeHubConnection()
		{
			return new HubConnectionWrapper(new HubConnection(_serverUrl));
		}

		[CLSCompliant(false)]
		protected virtual ILog MakeLogger()
		{
			return LogManager.GetLogger(typeof(SignalSender));
		}

		[CLSCompliant(false)]
		protected virtual void StartWorkerThread()
		{
			workerThread = new Thread(processQueue) { IsBackground = true };
			workerThread.Start();
		}





		public void SendData(DateTime floor, DateTime ceiling, Guid moduleId, Guid domainObjectId, Type domainInterfaceType, string dataSource, Guid businessUnitId)
		{
			var notification = new Notification
				{
					StartDate = Subscription.DateToString(floor),
					EndDate = Subscription.DateToString(ceiling),
					DomainId = Subscription.IdToString(domainObjectId),
					DomainQualifiedType = domainInterfaceType.AssemblyQualifiedName,
					DomainType = domainInterfaceType.Name,
					ModuleId = Subscription.IdToString(moduleId),
					DomainUpdateType = (int) DomainUpdateType.Insert,
					DataSource = dataSource,
					BusinessUnitId = Subscription.IdToString(businessUnitId),
					BinaryData = null
				};

			SendNotification(notification);
		}


		public void QueueRtaNotification(Guid personId, Guid businessUnitId, IActualAgentState actualAgentState)
		{
			var domainObject = JsonConvert.SerializeObject(actualAgentState);
			var type = typeof(IActualAgentState);

			var notification = new Notification
			{
				StartDate =
					Subscription.DateToString(actualAgentState.ReceivedTime.Add(actualAgentState.TimeInState.Negate())),
				EndDate = Subscription.DateToString(actualAgentState.ReceivedTime),
				DomainId = Subscription.IdToString(personId),
				DomainType = type.Name,
				DomainQualifiedType = type.AssemblyQualifiedName,
				ModuleId = Subscription.IdToString(Guid.Empty),
				DomainUpdateType = (int)DomainUpdateType.Insert,
				BinaryData = Convert.ToBase64String(Encoding.UTF8.GetBytes(domainObject)),
				BusinessUnitId = Subscription.IdToString(businessUnitId)
			};
			SendNotificationAsync(notification);
		}

	}
}