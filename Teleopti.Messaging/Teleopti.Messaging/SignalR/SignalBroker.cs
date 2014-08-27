using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Exceptions;
using log4net;
using Teleopti.Messaging.SignalR.Wrappers;
using Subscription = Teleopti.Interfaces.MessageBroker.Subscription;

namespace Teleopti.Messaging.SignalR
{
	public class SignalBroker : IMessageBroker
	{
		private readonly IEnumerable<IConnectionKeepAliveStrategy> _connectionKeepAliveStrategy;
		private readonly ITime _time;
		private IHandleHubConnection _connection;
		private SignalBrokerCommands _signalBrokerCommands;
		private readonly object _wrapperLock = new object();
		private static readonly ILog Logger = LogManager.GetLogger(typeof (SignalBroker));
		private MessageBrokerSender _messageBrokerSender;
		private MessageBrokerListener _messageBrokerListener;

		public static SignalBroker MakeForTest(IMessageFilterManager typeFilter)
		{
			return new SignalBroker(typeFilter, new IConnectionKeepAliveStrategy[] {}, new Time(new Now()));
		}

		public static SignalBroker Make(IMessageFilterManager typeFilter)
		{
			return new SignalBroker(typeFilter,
				new IConnectionKeepAliveStrategy[] {new RestartOnClosed(), new RecreateOnNoPingReply()}, new Time(new Now()));
		}

		public SignalBroker(IMessageFilterManager typeFilter, IEnumerable<IConnectionKeepAliveStrategy> connectionKeepAliveStrategy, ITime time)
		{
			_connectionKeepAliveStrategy = connectionKeepAliveStrategy;
			_time = time;
			FilterManager = typeFilter;

			ServicePointManager.ServerCertificateValidationCallback = ignoreInvalidCertificate;
			ServicePointManager.DefaultConnectionLimit = 50;

            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
			if (e.Observed) return;
			Logger.Debug("An unobserved task failed.", e.Exception);
	        e.SetObserved();
        }

		private static bool ignoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
		{
			return true;
		}

	    private IMessageFilterManager FilterManager { get; set; }

		public void StartMessageBroker(bool useLongPolling = false)
		{
			Uri serverUrl;
			if (!Uri.TryCreate(ConnectionString, UriKind.Absolute, out serverUrl))
			{
				throw new BrokerNotInstantiatedException("The SignalBroker can only be used with a valid Uri!");
			}

			lock (_wrapperLock)
			{
				var connection = new SignalConnection(
					() => MakeHubConnection(serverUrl),
					() => _messageBrokerListener.ReregisterSubscriptions(),
					_connectionKeepAliveStrategy,
					_time);

				_connection = connection;

				_signalBrokerCommands = new SignalBrokerCommands(Logger, connection);
				_messageBrokerSender = new MessageBrokerSender(_signalBrokerCommands, FilterManager);
				_messageBrokerListener = new MessageBrokerListener(_signalBrokerCommands);

				_connection.StartConnection(_messageBrokerListener.OnNotification, useLongPolling);
			}
		}

		public void Dispose()
		{
			lock (_wrapperLock)
			{
				if (_connection == null) return;

				_connection.CloseConnection();
				_connection = null;
			}
		}

	    public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			_messageBrokerSender.Send(dataSource, businessUnitId, eventStartDate, eventEndDate, moduleId, referenceObjectId, referenceObjectType, domainObjectId, domainObjectType, updateType, domainObject);
		}

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			_messageBrokerSender.Send(dataSource, businessUnitId, eventStartDate, eventEndDate, moduleId, domainObjectId, domainObjectType, updateType, domainObject);
		}

		public void Send(string dataSource, Guid businessUnitId, IEventMessage[] eventMessages)
		{
			_messageBrokerSender.Send(dataSource, businessUnitId, eventMessages);
		}

		public void SendNotification(Notification notification)
		{
			if (_connection != null)
				_signalBrokerCommands.NotifyClients(notification);
		}

		public void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType)
		{
			_messageBrokerListener.RegisterEventSubscription(dataSource, businessUnitId, eventMessageHandler, domainObjectType);
		}

		public void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Type domainObjectType)
		{
			_messageBrokerListener.RegisterEventSubscription(dataSource, businessUnitId, eventMessageHandler, referenceObjectId, referenceObjectType, domainObjectType);
		}

		public void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			_messageBrokerListener.RegisterEventSubscription(dataSource, businessUnitId, eventMessageHandler, domainObjectType, startDate, endDate);
		}

		public void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			_messageBrokerListener.RegisterEventSubscription(dataSource, businessUnitId, eventMessageHandler, domainObjectId, domainObjectType, startDate, endDate);
		}

		public void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			_messageBrokerListener.RegisterEventSubscription(dataSource, businessUnitId, eventMessageHandler, referenceObjectId, referenceObjectType, domainObjectType, startDate, endDate);
		}

		public void UnregisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			_messageBrokerListener.UnregisterEventSubscription(eventMessageHandler);
		}

		[CLSCompliant(false)]
		protected virtual IHubConnectionWrapper MakeHubConnection(Uri serverUrl)
		{
			return new HubConnectionWrapper(new HubConnection(serverUrl.ToString()));
		}

		public void StopMessageBroker()
		{
			lock (_wrapperLock)
			{
				if (_connection != null)
				{
					_connection.CloseConnection();
				}
			}
		}

		public string ConnectionString { get; set; }

		public bool IsConnected
		{
			get
			{
				return _connection!=null && _connection.IsConnected();
			}
		}

	}

	public class SubscriptionCallback
	{
		public Subscription Subscription { get; set; }
		public EventHandler<EventMessageArgs> Callback { get; set; }
	}
}
