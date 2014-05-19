using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;
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
		private readonly IList<SubscriptionCallback> _subscriptions = new List<SubscriptionCallback>();
		private IHandleHubConnection _connection;
		private ISignalBrokerCommands _signalBrokerCommands;
		private readonly object _wrapperLock = new object();
		private static readonly ILog Logger = LogManager.GetLogger(typeof (SignalBroker));

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
			IsTypeFilterApplied = true;

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

		public void StartMessageBroker()
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
					() =>
					{
						foreach (var subscription in _subscriptions)
							_signalBrokerCommands.AddSubscription(subscription.Subscription);
					},
					_connectionKeepAliveStrategy,
					_time);

				_connection = connection;

				_signalBrokerCommands = new SignalBrokerCommands(Logger, connection);

				_connection.StartConnection(onNotification);
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

	    private IEnumerable<Notification> createNotifications(string dataSource, string businessUnitId,
	                                                          DateTime eventStartDate, DateTime eventEndDate, Guid moduleId,
	                                                          Guid referenceObjectId, Type referenceObjectType,
	                                                          Guid domainObjectId, Type domainObjectType,
	                                                          DomainUpdateType updateType, byte[] domainObject)
        {
	        if (FilterManager.HasType(domainObjectType))
	        {
	            var referenceObjectTypeString = (referenceObjectType == null)
	                                                ? null
	                                                : FilterManager.LookupTypeToSend(referenceObjectType);
	            var eventStartDateString = Subscription.DateToString(eventStartDate);
	            var eventEndDateString = Subscription.DateToString(eventEndDate);
	            var moduleIdString = Subscription.IdToString(moduleId);
	            var domainObjectIdString = Subscription.IdToString(domainObjectId);
	            var domainQualifiedTypeString = FilterManager.LookupTypeToSend(domainObjectType);
	            var domainReferenceIdString = Subscription.IdToString(referenceObjectId);
	            var domainObjectString = (domainObject != null) ? Convert.ToBase64String(domainObject) : null;
	            yield return new Notification
	                {
	                    StartDate = eventStartDateString,
	                    EndDate = eventEndDateString,
	                    DomainId = domainObjectIdString,
	                    DomainType = FilterManager.LookupType(domainObjectType).Name,
	                    DomainQualifiedType = domainQualifiedTypeString,
	                    DomainReferenceId = domainReferenceIdString,
	                    DomainReferenceType = referenceObjectTypeString,
	                    ModuleId = moduleIdString,
	                    DomainUpdateType = (int) updateType,
	                    DataSource = dataSource,
	                    BusinessUnitId = businessUnitId,
	                    BinaryData = domainObjectString
	                };
	        }
        }

	    public void SendEventMessage(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			var notificationList = createNotifications(dataSource, Subscription.IdToString(businessUnitId), eventStartDate,
			                                           eventEndDate, moduleId, referenceObjectId,
			                                           referenceObjectType, domainObjectId, domainObjectType, updateType,
			                                           domainObject);
			_signalBrokerCommands.NotifyClients(notificationList);
		}

		public void SendEventMessage(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			SendEventMessage(dataSource, businessUnitId, eventStartDate, eventEndDate, moduleId, Guid.Empty, null, domainObjectId, domainObjectType, updateType, domainObject);
		}

		public void SendEventMessages(string dataSource, Guid businessUnitId, IEventMessage[] eventMessages)
		{
			var notificationList = new List<Notification>();
			var businessUnitIdString = Subscription.IdToString(businessUnitId);
			foreach (var eventMessage in eventMessages)
			{
				notificationList.AddRange(createNotifications(dataSource, businessUnitIdString, eventMessage.EventStartDate,
				                                              eventMessage.EventEndDate, eventMessage.ModuleId,
				                                              eventMessage.ReferenceObjectId, eventMessage.ReferenceObjectTypeCache,
				                                              eventMessage.DomainObjectId,
				                                              eventMessage.DomainObjectTypeCache, eventMessage.DomainUpdateType,
				                                              eventMessage.DomainObject));

				if (notificationList.Count > 200)
				{
					_signalBrokerCommands.NotifyClients(notificationList);
					notificationList.Clear();
				}
			}
			if (notificationList.Count > 0)
				_signalBrokerCommands.NotifyClients(notificationList);
		}

		public void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType)
		{
			registerEventSubscription(dataSource, businessUnitId, eventMessageHandler, null, null, null, domainObjectType, Consts.MinDate, Consts.MaxDate);
		}

		public void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Type domainObjectType)
		{
			registerEventSubscription(dataSource, businessUnitId, eventMessageHandler, referenceObjectId, referenceObjectType, null, domainObjectType, Consts.MinDate, Consts.MaxDate);
		}

		public void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			registerEventSubscription(dataSource, businessUnitId, eventMessageHandler, null, null, null, domainObjectType, startDate, endDate);
		}

		public void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			registerEventSubscription(dataSource, businessUnitId, eventMessageHandler, null, null, domainObjectId, domainObjectType, startDate, endDate);
		}

		private void registerEventSubscription(string datasource, Guid businessUnitId,
			EventHandler<EventMessageArgs> eventMessageHandler, Guid? referenceObjectId, Type referenceObjectType,
			Guid? domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			//It is mad that this one is here! But it is "inherited" from the old broker. So it must be here to avoid bugs when running with the web broker only.
			if (!domainObjectType.IsInterface)
				throw new NotInterfaceTypeException();

			var subscription = new Subscription
			{
				DomainId = domainObjectId.HasValue ? Subscription.IdToString(domainObjectId.Value) : null,
				DomainType = domainObjectType.Name,
				DomainReferenceId = referenceObjectId.HasValue ? Subscription.IdToString(referenceObjectId.Value) : null,
				DomainReferenceType =
					(referenceObjectType == null) ? null : referenceObjectType.AssemblyQualifiedName,
				LowerBoundary = Subscription.DateToString(startDate),
				UpperBoundary = Subscription.DateToString(endDate),
				DataSource = datasource,
				BusinessUnitId = Subscription.IdToString(businessUnitId),
			};

			lock (_wrapperLock)
			{
				if (_connection == null) return;

				_subscriptions.Add(new SubscriptionCallback
				{
					Subscription = subscription,
					Callback = eventMessageHandler
				});
				_signalBrokerCommands.AddSubscription(subscription);
			}
		}

		public void UnregisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			// cleanup refactoring, but keeping the same buggy behavior: does not remove subscription from the server.
			// should also do this somewhere, when there are no local routes left:
			//_signalBrokerCommands.RemoveSubscription(route);
			// if you want more information check hg history

			lock (_wrapperLock)
			{
				if (_connection == null) return;

				var toRemove = (from s in _subscriptions
					where s.Callback == eventMessageHandler
					select s).ToList();

				toRemove.ForEach(s => _subscriptions.Remove(s));
			}
		}

		public void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			registerEventSubscription(dataSource, businessUnitId, eventMessageHandler, referenceObjectId, referenceObjectType, null, domainObjectType, startDate, endDate);
		}

		[CLSCompliant(false)]
		protected virtual IHubConnectionWrapper MakeHubConnection(Uri serverUrl)
		{
			return new HubConnectionWrapper(new HubConnection(serverUrl.ToString()));
		}

		private void onNotification(Notification d)
		{
			var message = new EventMessage
				{
					InterfaceType = Type.GetType(d.DomainQualifiedType, false, true),
					DomainObjectType = d.DomainType,
					DomainObjectId = d.DomainIdAsGuid(),
					ModuleId = d.ModuleIdAsGuid(),
					ReferenceObjectId = d.DomainReferenceIdAsGuid(),
					ReferenceObjectType = d.DomainReferenceType,
					EventStartDate = d.StartDateAsDateTime(),
					EventEndDate = d.EndDateAsDateTime(),
					DomainUpdateType = d.DomainUpdateTypeAsDomainUpdateType()
				};

			var domainObject = d.BinaryData;
			if (!string.IsNullOrEmpty(domainObject))
			{
				message.DomainObject = Convert.FromBase64String(domainObject);
			}

			InvokeEventHandlers(message, d.Routes());
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

	    private bool IsTypeFilterApplied { get; set; }

		public bool IsConnected
		{
			get
			{
				return _connection!=null && _connection.IsConnected();
			}
		}

		private void InvokeEventHandlers(EventMessage eventMessage, IEnumerable<string> routes)
		{
			// locks everywhere in this class, but no lock here?!!!
			// if locks are even required, should be here aswell no?

			var matchingHandlers = from s in _subscriptions
				from r in routes
				let route = s.Subscription.Route()
				where
					route == r &&
					s.Subscription.LowerBoundaryAsDateTime() <= eventMessage.EventEndDate &&
					s.Subscription.UpperBoundaryAsDateTime() >= eventMessage.EventStartDate
				select s.Callback;

			foreach (var handler in matchingHandlers)
				handler(this, new EventMessageArgs(eventMessage));

		}
	}

	public class SubscriptionCallback
	{
		public Subscription Subscription { get; set; }
		public EventHandler<EventMessageArgs> Callback { get; set; }
	}
}
