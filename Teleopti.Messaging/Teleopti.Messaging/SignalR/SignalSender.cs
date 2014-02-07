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
		private string _serverUrl;
		private ISignalWrapper _wrapper;
		private readonly BlockingCollection<Tuple<DateTime, Notification>> _notificationQueue = new BlockingCollection<Tuple<DateTime, Notification>>();
		private Thread workerThread;
		private readonly CancellationTokenSource cancelToken = new CancellationTokenSource();
		protected ILog _logger;

		public SignalSender(string serverUrl)
		{
			_logger = MakeLogger();
			_serverUrl = serverUrl;

			ServicePointManager.ServerCertificateValidationCallback = ignoreInvalidCertificate;
			ServicePointManager.DefaultConnectionLimit = 50;

            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
			StartWorkerThread();
		}

		public void InstantiateBrokerService()
		{
			try
			{
				if (_wrapper != null) _wrapper.StopHub();

				var connection = MakeHubConnection();
				var proxy = connection.CreateHubProxy("MessageBrokerHub");

				_wrapper = new SignalWrapper(proxy, connection, _logger);
				_wrapper.StartHub();
			}
			catch (SocketException exception)
			{
				_logger.Error("The message broker seems to be down.", exception);
			}
			catch (BrokerNotInstantiatedException exception)
			{
				_logger.Error("The message broker seems to be down.", exception);
			}
		}

		private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
	    {
	        if (!e.Observed)
            {
                _logger.Error("An error occured, please review the error and take actions necessary.", e.Exception);
                e.SetObserved();
            }
	    }

	    private static bool ignoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
		{
			return true;
		}

		public void Dispose()
		{
			cancelToken.Cancel();
			workerThread.Join();
			_wrapper.StopHub();
			_wrapper = null;
		}

		public bool IsAlive
		{
			get { return _wrapper != null && _wrapper.IsInitialized(); }
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

			var retryCount = 1;
			while (retryCount <= 3)
			{
				if (trySend(retryCount, notifications)) break;
				retryCount++;
			}
			if (retryCount > 3)
				_logger.Error("Could not send batch messages.");
		}

		private bool trySend(int attemptNumber, IEnumerable<Notification> notifications)
		{
			try
			{
				Exception exception = null;
				var task = _wrapper.NotifyClients(notifications);
				task.ContinueWith(t =>
					{
						if (t.IsFaulted && t.Exception != null)
						{
							exception = t.Exception.GetBaseException();
							_logger.Error("An error happened when notifying multiple.", exception);
						}
					}, TaskContinuationOptions.OnlyOnFaulted);
				var waitResult = task.Wait(1000, cancelToken.Token);
				if (exception != null)
					throw exception;

				return waitResult;
			}
			catch (Exception e)
			{
				Thread.Sleep(250 * attemptNumber);
				InstantiateBrokerService();
			}
			return false;
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
			var sendAttempt = 0;
			while (sendAttempt < 3)
			{
				try
				{
					sendAttempt++;

					var task = _wrapper.NotifyClients(new Notification
					{
						StartDate = Subscription.DateToString(floor),
						EndDate = Subscription.DateToString(ceiling),
						DomainId = Subscription.IdToString(domainObjectId),
						DomainQualifiedType = domainInterfaceType.AssemblyQualifiedName,
						DomainType = domainInterfaceType.Name,
						ModuleId = Subscription.IdToString(moduleId),
						DomainUpdateType = (int)DomainUpdateType.Insert,
						DataSource = dataSource,
						BusinessUnitId = Subscription.IdToString(businessUnitId),
						BinaryData = null
					});
					task.Wait(TimeSpan.FromSeconds(20));
					break;
				}
				catch (Exception)
				{
					InstantiateBrokerService();
				}
			}
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