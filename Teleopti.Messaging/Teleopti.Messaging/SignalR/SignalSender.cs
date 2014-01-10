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
		private ISignalWrapper _wrapper;
		private readonly BlockingCollection<Tuple<DateTime, Notification>> _notificationQueue = new BlockingCollection<Tuple<DateTime, Notification>>();
		private readonly Thread workerThread;
		private readonly CancellationTokenSource cancelToken = new CancellationTokenSource();
		private static readonly ILog Logger = LogManager.GetLogger(typeof (SignalSender));
		private bool _queueProcessed;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
		public SignalSender(string serverUrl)
		{
			_serverUrl = serverUrl;

			ServicePointManager.ServerCertificateValidationCallback = ignoreInvalidCertificate;
			ServicePointManager.DefaultConnectionLimit = 50;

            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
			workerThread = new Thread(processQueue) {IsBackground = true};
			workerThread.Start();
		}

	    private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
	    {
	        if (!e.Observed)
            {
                Logger.Error("An error occured, please review the error and take actions necessary.", e.Exception);
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

		public void QueueRtaNotification(Guid personId, Guid businessUnitId, IActualAgentState actualAgentState)
		{
			var domainObject = JsonConvert.SerializeObject(actualAgentState);
			var type = typeof (IActualAgentState);

			var notification = new Notification
				{
					StartDate =
						Subscription.DateToString(actualAgentState.ReceivedTime.Add(actualAgentState.TimeInState.Negate())),
					EndDate = Subscription.DateToString(actualAgentState.ReceivedTime),
					DomainId = Subscription.IdToString(personId),
					DomainType = type.Name,
					DomainQualifiedType = type.AssemblyQualifiedName,
					ModuleId = Subscription.IdToString(Guid.Empty),
					DomainUpdateType = (int) DomainUpdateType.Insert,
					BinaryData = Convert.ToBase64String(Encoding.UTF8.GetBytes(domainObject)),
					BusinessUnitId = Subscription.IdToString(businessUnitId)
				};
			_queueProcessed = false;
			_notificationQueue.Add(new Tuple<DateTime, Notification>(DateTime.UtcNow, notification));
		}

		private void processQueue(object state)
		{
			while (!cancelToken.IsCancellationRequested)
			{
				var notifications =
					_notificationQueue.GetConsumingEnumerable(cancelToken.Token)
					                  .Take(Math.Max(1,Math.Min(_notificationQueue.Count,20)))
					                  .ToArray()
					                  .Where(t => t.Item1 > DateTime.UtcNow.AddMinutes(-2))
									  .Select(t => t.Item2)
									  .ToArray();
				
				if (!notifications.Any()) continue;
				
				var retryCount = 1;
				while (retryCount<=3)
				{
					if (trySend(retryCount, notifications)) break;
					retryCount++;
				}
				if (retryCount > 3)
					Logger.Error("Could not send batch messages.");

				if (_notificationQueue.Count == 0)
					_queueProcessed = true;
			}
		}

		public void WaitUntilQueueProcessed()
		{
			while (!_queueProcessed)
			{
			}
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
							Logger.Error("An error happened when notifying multiple.", exception);
						}
					}, TaskContinuationOptions.OnlyOnFaulted);
				var waitResult = task.Wait(1000, cancelToken.Token);
				if (exception != null)
					throw exception;

				return waitResult;
			}
			catch (Exception)
			{
				Thread.Sleep(250 * attemptNumber);
				InstantiateBrokerService();
			}
			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4")]
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
					          		DomainUpdateType = (int) DomainUpdateType.Insert,
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

		public void InstantiateBrokerService()
		{
			try
			{
				if (_wrapper != null) _wrapper.StopHub();

				var connection = MakeHubConnection();
				var proxy = connection.CreateHubProxy("MessageBrokerHub");

				_wrapper = new SignalWrapper(proxy, connection);
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
}