using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using SignalR.Client.Hubs;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Composites;
using Teleopti.Messaging.Events;
using Teleopti.Messaging.Exceptions;
using Subscription = Teleopti.Interfaces.MessageBroker.Subscription;

namespace Teleopti.Messaging.SignalR
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public class SignalBroker : IMessageBroker
	{
		private const string HubClassName = "MessageBrokerHub";
		private readonly IDictionary<string, IList<SubscriptionWithHandler>> _subscriptionHandlers = new Dictionary<string, IList<SubscriptionWithHandler>>();
		private SignalWrapper _wrapper;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public SignalBroker(IDictionary<Type, IList<Type>> typeFilter)
		{
			FilterManager = new MessageFilterManager();
			FilterManager.InitializeTypeFilter(typeFilter);
			IsTypeFilterApplied = true;

			ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ignoreInvalidCertificate);
		}

		private static bool ignoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
		{
			return true;
		}

		public IMessageFilterManager FilterManager { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
		public void Dispose()
		{
			_wrapper.StopListening();
			_wrapper = null;
		}

		public void SendEventMessage(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
		{
			SendEventMessage(dataSource, businessUnitId, eventStartDate, eventEndDate, moduleId, referenceObjectId, referenceObjectType, domainObjectId, domainObjectType, updateType, null);
		}

		public void SendEventMessage(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			IList<Type> types;
			if (FilterManager.FilterDictionary.TryGetValue(domainObjectType, out types))
			{
				foreach (var type in types)
				{
					callProxy(new Notification
					{
						StartDate = Subscription.DateToString(eventStartDate),
						EndDate = Subscription.DateToString(eventEndDate),
						DomainId = Subscription.IdToString(domainObjectId),
						DomainType = type.Name,
						DomainQualifiedType =  types[0].AssemblyQualifiedName,
						DomainReferenceId = Subscription.IdToString(referenceObjectId),
						DomainReferenceType =
							(referenceObjectType == null)
								? null
								: FilterManager.LookupType(referenceObjectType),
						ModuleId = Subscription.IdToString(moduleId),
						DomainUpdateType = (int)updateType,
						DataSource = dataSource,
						BusinessUnitId = Subscription.IdToString(businessUnitId),
						BinaryData =
							(domainObject != null) ? Encoding.UTF8.GetString(domainObject) : null
					});
				}
			}
		}

		private void callProxy(Notification state)
		{
			_wrapper.NotifyClients(state);
		}

		public void SendEventMessage(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			SendEventMessage(dataSource, businessUnitId, eventStartDate, eventEndDate, moduleId, Guid.Empty, null, domainObjectId, domainObjectType, updateType, domainObject);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public void SendEventMessages(string dataSource, Guid businessUnitId, IEventMessage[] eventMessages)
		{
			foreach (var eventMessage in eventMessages)
			{
				SendEventMessage(dataSource,businessUnitId, eventMessage.EventStartDate, eventMessage.EventEndDate, eventMessage.ModuleId,
								 eventMessage.ReferenceObjectId, eventMessage.ReferenceObjectTypeCache, eventMessage.DomainObjectId,
								 eventMessage.DomainObjectTypeCache, eventMessage.DomainUpdateType, eventMessage.DomainObject);
			}
		}

		public void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType)
		{
			registerEventSubscription(dataSource, businessUnitId, eventMessageHandler, null, null, null, domainObjectType, Consts.MinDate, Consts.MaxDate);
		}

		public void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid domainObjectId, Type domainObjectType)
		{
			registerEventSubscription(dataSource, businessUnitId, eventMessageHandler, null, null, domainObjectId, domainObjectType, Consts.MinDate,
			                          Consts.MaxDate);
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4")]
		private void registerEventSubscription(string datasource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid? referenceObjectId, Type referenceObjectType, Guid? domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate)
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

			_wrapper.AddSubscription(subscription).Then(_ =>
			{
				var route = subscription.Route();

				IList<SubscriptionWithHandler> handlers;
				if (!_subscriptionHandlers.TryGetValue(route, out handlers))
				{
					handlers = new List<SubscriptionWithHandler>();
					_subscriptionHandlers.Add(route, handlers);
				}
				handlers.Add(new SubscriptionWithHandler { Handler = eventMessageHandler, Subscription = subscription });
			});
		}

		public void UnregisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			if (_wrapper == null) return;

			var handlersToRemove = new List<string>();
			var subscriptionWithHandlersToRemove = new List<SubscriptionWithHandler>();
			foreach (var subscriptionHandler in _subscriptionHandlers)
			{
				foreach (var subscriptionWithHandler in subscriptionHandler.Value)
				{
					var target = subscriptionWithHandler.Handler;
					if (target == eventMessageHandler)
					{
						subscriptionWithHandlersToRemove.Add(subscriptionWithHandler);
						if (subscriptionHandler.Value.Count==0)
						{
							var route = subscriptionWithHandler.Subscription.Route();
							_wrapper.RemoveSubscription(route);
							handlersToRemove.Add(route);
						}
					}
				}

				foreach (var subscriptionWithHandler in subscriptionWithHandlersToRemove)
				{
					subscriptionHandler.Value.Remove(subscriptionWithHandler);
				}
			}

			foreach (var route in handlersToRemove)
			{
				_subscriptionHandlers.Remove(route);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public IEventMessage CreateEventMessage(Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
		{
			return new EventMessage
			{
				ModuleId = moduleId,
				DomainObjectId = domainObjectId,
				DomainObjectType = domainObjectType.AssemblyQualifiedName,
				DomainUpdateType = updateType,
				EventStartDate = Consts.MinDate,
				EventEndDate = Consts.MaxDate
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4")]
		public IEventMessage CreateEventMessage(Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
		{
			return new EventMessage
			{
				ModuleId = moduleId,
				DomainObjectId = domainObjectId,
				DomainObjectType = domainObjectType.AssemblyQualifiedName,
				DomainUpdateType = updateType,
				ReferenceObjectId = referenceObjectId,
				ReferenceObjectType = referenceObjectType.AssemblyQualifiedName,
				EventStartDate = Consts.MinDate,
				EventEndDate = Consts.MaxDate
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4")]
		public IEventMessage CreateEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
		{
			return new EventMessage
			{
				ModuleId = moduleId,
				DomainObjectId = domainObjectId,
				DomainObjectType = domainObjectType.AssemblyQualifiedName,
				DomainUpdateType = updateType,
				EventStartDate = eventStartDate,
				EventEndDate = eventEndDate
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "6")]
		public IEventMessage CreateEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
		{
			return new EventMessage
			{
				ModuleId = moduleId,
				DomainObjectId = domainObjectId,
				DomainObjectType = domainObjectType.AssemblyQualifiedName,
				DomainUpdateType = updateType,
				EventStartDate = eventStartDate,
				EventEndDate = eventEndDate,
				ReferenceObjectId = referenceObjectId,
				ReferenceObjectType = referenceObjectType.AssemblyQualifiedName,
			};
		}

		public IList<IConfigurationInfo> RetrieveConfigurations()
		{
			return new List<IConfigurationInfo>();
		}

		public IList<IMessageInformation> RetrieveAddresses()
		{
			return new List<IMessageInformation>();
		}

		public void UpdateConfigurations(IList<IConfigurationInfo> configurations)
		{
		}

		public void DeleteConfigurationItem(IConfigurationInfo configurationInfo)
		{
		}

		public void UpdateAddresses(IList<IMessageInformation> addresses)
		{
		}

		public void DeleteAddressItem(IMessageInformation addressInfo)
		{
		}

		public IEventHeartbeat[] RetrieveHeartbeats()
		{
			return new IEventHeartbeat[] { };
		}

		public ILogbookEntry[] RetrieveLogbookEntries()
		{
			return new ILogbookEntry[] { };
		}

		public IEventUser[] RetrieveEventUsers()
		{
			return new IEventUser[] { };
		}

		public IEventReceipt[] RetrieveEventReceipt()
		{
			return new IEventReceipt[] { };
		}

		public IEventSubscriber[] RetrieveSubscribers()
		{
			return new IEventSubscriber[] { };
		}

		public IEventFilter[] RetrieveFilters()
		{
			return new IEventFilter[] { };
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SignalBroker")]
		public void StartMessageBroker()
		{
			Uri serverUrl;
			if (!Uri.TryCreate(ConnectionString, UriKind.Absolute, out serverUrl))
			{
				throw new BrokerNotInstantiatedException("The SignalBroker can only be used with a valid Uri!");
			}
			var connection = new HubConnection(serverUrl.ToString());
			var hubProxy = connection.CreateProxy(HubClassName);

			_wrapper = new SignalWrapper(hubProxy, connection);
			_wrapper.OnNotification += onNotification;
			_wrapper.StartListening();
		}

		private void onNotification(Notification d)
		{
			var message = new EventMessage();
			message.InterfaceType = Type.GetType(d.DomainQualifiedType, false, true);
			message.DomainObjectType = d.DomainType;
			message.DomainObjectId = d.DomainIdAsGuid();
			message.ModuleId = d.ModuleIdAsGuid();
			message.ReferenceObjectId = d.DomainReferenceIdAsGuid();
			message.ReferenceObjectType = d.DomainReferenceType;
			message.EventStartDate = d.StartDateAsDateTime();
			message.EventEndDate = d.EndDateAsDateTime();
			message.DomainUpdateType = d.DomainUpdateTypeAsDomainUpdateType();

			var domainObject = d.BinaryData;
			if (!string.IsNullOrEmpty(domainObject))
			{
				message.DomainObject = Encoding.UTF8.GetBytes(domainObject);
			}

			InvokeEventHandlers(message, d.Routes());
		}

		public void StopMessageBroker()
		{
			_wrapper.OnNotification -= onNotification;
			_wrapper.StopListening();
		}

		public int Initialized { get; set; }

		public int RemotingPort { get; set; }

		public int Threads { get; set; }

		public int UserId { get; set; }

		public MessagingProtocol MessagingProtocol { get; set; }

		public int MessagingPort { get; set; }

		public string Server { get; set; }

		public string ConnectionString { get; set; }

		public Guid SubscriberId { get; set; }

		public bool IsTypeFilterApplied { get; set; }

		public bool IsInitialized
		{
			get { return _wrapper!=null && _wrapper.IsInitialized(); }
		}

		public event EventHandler<EventMessageArgs> EventMessageHandler;

		public void InvokeEventMessageHandler(EventMessageArgs e)
		{
			EventHandler<EventMessageArgs> handler = EventMessageHandler;
			if (handler != null) handler(this, e);
		}

		public event EventHandler<UnhandledExceptionEventArgs> ExceptionHandler;

		public void InvokeExceptionHandler(UnhandledExceptionEventArgs e)
		{
			EventHandler<UnhandledExceptionEventArgs> handler = ExceptionHandler;
			if (handler != null) handler(this, e);
		}

		public bool Restart()
		{
			StopMessageBroker();
			StartMessageBroker();
			return true;
		}

		public void Log(ILogEntry eventLogEntry)
		{
		}

		public void SendReceipt(IEventMessage message)
		{
		}

		public IMessageInformation[] CreateAddresses()
		{
			return new IMessageInformation[] { };
		}

		public IConfigurationInfo[] CreateConfigurations()
		{
			return new IConfigurationInfo[] { };
		}

		public void InternalLog(Exception exception)
		{
		}

		public void ServiceGuard(IBrokerService service)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public void InvokeEventHandlers(EventMessage eventMessage, string[] routes)
		{
			foreach (var route in routes)
			{
				IList<SubscriptionWithHandler> reference;
				if (_subscriptionHandlers.TryGetValue(route, out reference))
				{
					foreach (var subscriptionWithHandler in reference)
					{
						if (subscriptionWithHandler.Subscription.LowerBoundaryAsDateTime() <= eventMessage.EventEndDate &&
						  subscriptionWithHandler.Subscription.UpperBoundaryAsDateTime() >= eventMessage.EventStartDate)
						{
							subscriptionWithHandler.Handler.Invoke(this, new EventMessageArgs(eventMessage));
						}
					}
				}
			}
		}
	}

	public class SubscriptionWithHandler
	{
		public Subscription Subscription { get; set; }
		public EventHandler<EventMessageArgs> Handler { get; set; }
	}
}
