using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using SignalR.Client._20.Hubs;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Composites;
using Teleopti.Messaging.Events;
using SignalR.Client._20.Transports;
using Teleopti.Messaging.Exceptions;
using Subscription = Teleopti.Interfaces.MessageBroker.Subscription;

namespace Teleopti.Messaging.SignalR
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public class SignalBroker : IMessageBroker
	{
		private const string EventName = "onEventMessage";
		private const string HubClassName = "Teleopti.Ccc.Web.Broker.MessageBrokerHub";
		private IHubProxy _proxy;
		private readonly IDictionary<Guid, EventHandler<EventMessageArgs>> _subscriptionHandlers = new Dictionary<Guid, EventHandler<EventMessageArgs>>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public SignalBroker(IDictionary<Type, IList<Type>> typeFilter)
		{
			FilterManager = new MessageFilterManager();
			FilterManager.InitializeTypeFilter(typeFilter);
			IsTypeFilterApplied = true;
		}

		public IMessageFilterManager FilterManager { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
		public void Dispose()
		{
			_proxy = null;
		}

		public void SendEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
		{
			SendEventMessage(eventStartDate, eventEndDate, moduleId, referenceObjectId, referenceObjectType, domainObjectId, domainObjectType, updateType, null);
		}

		public void SendEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			ThreadPool.QueueUserWorkItem(callProxy, new Notification
			{
				StartDate = Subscription.DateToString(eventStartDate),
				EndDate = Subscription.DateToString(eventEndDate),
				DomainId = Subscription.IdToString(domainObjectId),
				DomainType =
					(domainObjectType == null)
						? null
						: FilterManager.LookupType(domainObjectType),
				DomainReferenceId = Subscription.IdToString(referenceObjectId),
				DomainReferenceType =
					(referenceObjectType == null)
						? null
						: FilterManager.LookupType(referenceObjectType),
				ModuleId = Subscription.IdToString(moduleId),
				DomainUpdateType = (int)updateType,
				BinaryData =
					(domainObject != null) ? Encoding.UTF8.GetString(domainObject) : null
			});
		}

		private void callProxy(object state)
		{
			_proxy.Invoke("NotifyClients", (Notification)state);
		}

		public void SendEventMessage(Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType)
		{
			SendEventMessage(Consts.MinDate, Consts.MaxDate, Guid.Empty, referenceObjectId, referenceObjectType, domainObjectId, domainObjectType, DomainUpdateType.NotApplicable, null);
		}

		public void SendEventMessage(Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
		{
			SendEventMessage(Consts.MinDate, Consts.MaxDate, Guid.Empty, referenceObjectId, referenceObjectType, domainObjectId, domainObjectType, updateType, null);
		}

		public void SendEventMessage(Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
		{
			SendEventMessage(Consts.MinDate, Consts.MaxDate, moduleId, referenceObjectId, referenceObjectType, domainObjectId, domainObjectType, updateType, null);
		}

		public void SendEventMessage(Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			SendEventMessage(Consts.MinDate, Consts.MaxDate, moduleId, referenceObjectId, referenceObjectType, domainObjectId, domainObjectType, updateType, domainObject);
		}

		public void SendEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
		{
			SendEventMessage(eventStartDate, eventEndDate, moduleId, Guid.Empty, null, domainObjectId, domainObjectType, updateType, null);
		}

		public void SendEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			SendEventMessage(eventStartDate, eventEndDate, moduleId, Guid.Empty, null, domainObjectId, domainObjectType, updateType, domainObject);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SendEventMessages(IEventMessage[] eventMessages)
		{
			foreach (var eventMessage in eventMessages)
			{
				SendEventMessage(eventMessage.EventStartDate, eventMessage.EventEndDate, eventMessage.ModuleId,
								 eventMessage.ReferenceObjectId, eventMessage.ReferenceObjectTypeCache, eventMessage.DomainObjectId,
								 eventMessage.DomainObjectTypeCache, eventMessage.DomainUpdateType, eventMessage.DomainObject);
			}
		}

		public void SendEventMessage(Guid domainObjectId, Type domainObjectType)
		{
			SendEventMessage(domainObjectId, domainObjectType, DomainUpdateType.NotApplicable);
		}

		public void SendEventMessage(Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
		{
			SendEventMessage(Guid.Empty, domainObjectId, domainObjectType, updateType);
		}

		public void SendEventMessage(Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
		{
			SendEventMessage(moduleId, domainObjectId, domainObjectType, updateType, null);
		}

		public void SendEventMessage(Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			SendEventMessage(Consts.MinDate, Consts.MaxDate, moduleId, Guid.Empty, null, domainObjectId, domainObjectType, updateType, domainObject);
		}

		public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType)
		{
			RegisterEventSubscription(eventMessageHandler, Guid.Empty, domainObjectType);
		}

		public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Guid domainObjectId, Type domainObjectType)
		{
			RegisterEventSubscription(eventMessageHandler, domainObjectId, null, domainObjectType);
		}

		public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Type domainObjectType)
		{
			RegisterEventSubscription(eventMessageHandler, referenceObjectId, referenceObjectType, Guid.Empty, domainObjectType);
		}

		public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			RegisterEventSubscription(eventMessageHandler, Guid.Empty, domainObjectType, startDate, endDate);
		}

		public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Guid domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			RegisterEventSubscription(eventMessageHandler, Guid.Empty, null, domainObjectId, domainObjectType, startDate, endDate);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4")]
		public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			//It is mad that this one is here! But it is "inherited" from the old broker. So it must be here to avoid bugs when running with the web broker only.
			if (!domainObjectType.IsInterface)
				throw new NotInterfaceTypeException();

			var subscription = new Subscription
			{
				DomainId = Subscription.IdToString(domainObjectId),
				DomainType = domainObjectType.AssemblyQualifiedName,
				DomainReferenceId = Subscription.IdToString(referenceObjectId),
				DomainReferenceType =
					(referenceObjectType == null) ? null : referenceObjectType.AssemblyQualifiedName,
				LowerBoundary = Subscription.DateToString(startDate),
				UpperBoundary = Subscription.DateToString(endDate)
			};

			//var weakReference = new WeakReference(eventMessageHandler,true);
			EventSignal<object> result = _proxy.Invoke("AddSubscription", subscription);
			result.Finished += (sender, e) =>
			{
				_subscriptionHandlers.Add(new Guid((string)e.Result), eventMessageHandler);
			};
		}

		public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Type referenceObjectType, Type domainObjectType)
		{
			RegisterEventSubscription(eventMessageHandler, Guid.Empty, null, Guid.Empty, domainObjectType);
		}

		public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType)
		{
			RegisterEventSubscription(eventMessageHandler, referenceObjectId, referenceObjectType, domainObjectId, domainObjectType, Consts.MinDate, Consts.MaxDate);
		}

		public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Type referenceObjectType, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			RegisterEventSubscription(eventMessageHandler, Guid.Empty, referenceObjectType, Guid.Empty, domainObjectType, startDate, endDate);
		}

		public void UnregisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			if (_proxy == null) return;

			foreach (var subscriptionHandler in _subscriptionHandlers)
			{
				var target = subscriptionHandler.Value.Target as EventHandler<EventMessageArgs>;
				if (target == eventMessageHandler)
				{
					_proxy.Invoke("RemoveSubscription", subscriptionHandler.Key);
					_subscriptionHandlers.Remove(subscriptionHandler.Key);
				}
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public IEventMessage CreateEventMessage(Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			return new EventMessage
			{
				ModuleId = moduleId,
				DomainObjectId = domainObjectId,
				DomainObjectType = domainObjectType.AssemblyQualifiedName,
				DomainUpdateType = updateType,
				DomainObject = domainObject,
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4")]
		public IEventMessage CreateEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			return new EventMessage
			{
				ModuleId = moduleId,
				DomainObjectId = domainObjectId,
				DomainObjectType = domainObjectType.AssemblyQualifiedName,
				DomainUpdateType = updateType,
				EventStartDate = eventStartDate,
				EventEndDate = eventEndDate,
				DomainObject = domainObject
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "6")]
		public IEventMessage CreateEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
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
				DomainObject = domainObject
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
			_proxy = connection.CreateProxy(HubClassName);
			var subscription = _proxy.Subscribe(EventName);
			subscription.Data += subscription_Data;

			connection.Start();
		}

		private void subscription_Data(object[] obj)
		{
			var d = ((JObject)obj[0]).ToObject<Notification>();
			var s = new Guid((string)obj[1]);

			var message = new EventMessage();
			message.InterfaceType = Type.GetType(d.DomainType, false, true);
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

			InvokeEventHandlers(message, s);
		}

		public void StopMessageBroker()
		{
			var proxy = (HubProxy)_proxy;
			var subscriptionList = new List<string>(proxy.GetSubscriptions());
			if (subscriptionList.Contains(EventName))
			{
				var subscription = _proxy.Subscribe(EventName);
				subscription.Data -= subscription_Data;
				proxy.RemoveEvent(EventName);
			}
		}

		public ISubscriber Subscriber { get; set; }

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
			get { return _proxy != null; }
		}

		public string ServicePath
		{
			get { return string.Empty; }
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public void InvokeEventHandlers(EventMessage eventMessage, Guid subscriptionId)
		{
			EventHandler<EventMessageArgs> reference;
			if (_subscriptionHandlers.TryGetValue(subscriptionId, out reference))
			{
				//if (reference.IsAlive)
				{
					reference.Invoke(this, new EventMessageArgs(eventMessage));
				}
			}
		}
	}
}
