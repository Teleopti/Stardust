using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;
using Teleopti.Messaging.Exceptions;
using log4net;
using Subscription = Teleopti.Interfaces.MessageBroker.Subscription;

namespace Teleopti.Messaging.SignalR
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public class SignalBroker : IMessageBroker
	{
		private const string HubClassName = "MessageBrokerHub";
		private readonly ConcurrentDictionary<string, IList<SubscriptionWithHandler>> _subscriptionHandlers = new ConcurrentDictionary<string, IList<SubscriptionWithHandler>>();
		private ISignalWrapper _wrapper;
		private SignalSubscriber _subscriberWrapper;
		private readonly object WrapperLock = new object();
		private static readonly ILog Logger = LogManager.GetLogger(typeof (SignalBroker));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public SignalBroker(IMessageFilterManager typeFilter)
		{
			FilterManager = typeFilter;
			IsTypeFilterApplied = true;

			ServicePointManager.ServerCertificateValidationCallback = ignoreInvalidCertificate;
			ServicePointManager.DefaultConnectionLimit = 50;

            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
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

		public IMessageFilterManager FilterManager { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
		public void Dispose()
		{
			lock (WrapperLock)
			{
				if (_wrapper == null) return;

				_wrapper.StopHub();
				_wrapper = null;
			}
		}

		public void SendEventMessage(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
		{
			SendEventMessage(dataSource, businessUnitId, eventStartDate, eventEndDate, moduleId, referenceObjectId, referenceObjectType, domainObjectId, domainObjectType, updateType, null);
		}

	    private IEnumerable<Notification> CreateNotifications(string dataSource, string businessUnitId,
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
			var notificationList = CreateNotifications(dataSource, Subscription.IdToString(businessUnitId), eventStartDate,
			                                           eventEndDate, moduleId, referenceObjectId,
			                                           referenceObjectType, domainObjectId, domainObjectType, updateType,
			                                           domainObject);
			var invoker = new HubProxyInvoker(callProxy);
			invoker.BeginInvoke(notificationList, callProxyCallback, invoker);
		}

		private void callProxy(IEnumerable<Notification> state)
		{
			lock (WrapperLock)
			{
				if (_wrapper == null) return;

				_wrapper.NotifyClients(state);
			}
		}

		public void SendEventMessage(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			SendEventMessage(dataSource, businessUnitId, eventStartDate, eventEndDate, moduleId, Guid.Empty, null, domainObjectId, domainObjectType, updateType, domainObject);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public void SendEventMessages(string dataSource, Guid businessUnitId, IEventMessage[] eventMessages)
		{
			var notificationList = new List<Notification>();
			var businessUnitIdString = Subscription.IdToString(businessUnitId);
			foreach (var eventMessage in eventMessages)
			{
				notificationList.AddRange(CreateNotifications(dataSource,businessUnitIdString, eventMessage.EventStartDate, eventMessage.EventEndDate, eventMessage.ModuleId,
								 eventMessage.ReferenceObjectId, eventMessage.ReferenceObjectTypeCache, eventMessage.DomainObjectId,
								 eventMessage.DomainObjectTypeCache, eventMessage.DomainUpdateType, eventMessage.DomainObject));

				if (notificationList.Count>200)
					{
					callProxy(notificationList);
					notificationList.Clear();
					}
			}
			if (notificationList.Count > 0)
			{
				var invoker = new HubProxyInvoker(callProxy);
				invoker.BeginInvoke(notificationList, callProxyCallback, invoker);
			}
		}

		private void callProxyCallback(IAsyncResult ar)
		{
			((HubProxyInvoker)ar.AsyncState).EndInvoke(ar);
		}

		private delegate void HubProxyInvoker(IEnumerable<Notification> notifications);

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

			lock (WrapperLock)
			{
				if (_wrapper == null) return;

				_wrapper.AddSubscription(subscription).ContinueWith(_ =>
				{
					var route = subscription.Route();

					var handlers = _subscriptionHandlers.GetOrAdd(route, key => new List<SubscriptionWithHandler>());
					handlers.Add(new SubscriptionWithHandler { Handler = eventMessageHandler, Subscription = subscription });
				});
			}
		}

		public void UnregisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			var handlersToRemove = new List<string>();

			lock (WrapperLock)
			{
				if (_wrapper == null) return;

				var subscriptionWithHandlersToRemove = new List<SubscriptionWithHandler>();
				var subscriptionValues = new List<IList<SubscriptionWithHandler>>(_subscriptionHandlers.Values);
				foreach (var subscriptionHandler in subscriptionValues)
				{
					if (subscriptionHandler == null)
					{
						continue;
					}

					foreach (var subscriptionWithHandler in subscriptionHandler)
					{
						var target = subscriptionWithHandler.Handler;
						if (target == eventMessageHandler)
						{
							subscriptionWithHandlersToRemove.Add(subscriptionWithHandler);
							if (subscriptionHandler.Count == 0 && subscriptionWithHandler.Subscription!=null)
							{
								var route = subscriptionWithHandler.Subscription.Route();
								_wrapper.RemoveSubscription(route);
								handlersToRemove.Add(route);
							}
						}
					}

					foreach (var subscriptionWithHandler in subscriptionWithHandlersToRemove)
					{
						subscriptionHandler.Remove(subscriptionWithHandler);
					}
				}
			}

			foreach (var route in handlersToRemove)
			{
				IList<SubscriptionWithHandler> removed;
				_subscriptionHandlers.TryRemove(route,out removed);
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SignalBroker")]
		public void StartMessageBroker()
		{
			Uri serverUrl;
			if (!Uri.TryCreate(ConnectionString, UriKind.Absolute, out serverUrl))
			{
				throw new BrokerNotInstantiatedException("The SignalBroker can only be used with a valid Uri!");
			}
			var connection = new HubConnection(serverUrl.ToString());
			var hubProxy = connection.CreateHubProxy(HubClassName);

			lock (WrapperLock)
			{
				_subscriberWrapper = new SignalSubscriber(hubProxy);
				_subscriberWrapper.OnNotification += onNotification;
				_subscriberWrapper.Start(); 
				
				_wrapper = new SignalWrapper(hubProxy, connection);
				_wrapper.StartHub();
			}
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
				message.DomainObject = Convert.FromBase64String(domainObject);
			}

			InvokeEventHandlers(message, d.Routes());
		}

		public void StopMessageBroker()
		{
			lock (WrapperLock)
			{
				if (_wrapper != null)
				{
					_wrapper.StopHub();
				}
				if (_subscriberWrapper != null)
				{
					_subscriberWrapper.Stop();
					_subscriberWrapper.OnNotification -= onNotification;
				}
			}
		}

		public int Initialized { get; set; }

		public int RemotingPort { get; set; }

		public int Threads { get; set; }

		public int UserId { get; set; }

		public int MessagingPort { get; set; }

		public string Server { get; set; }

		public string ConnectionString { get; set; }

		public Guid SubscriberId { get; set; }

		public bool IsTypeFilterApplied { get; set; }

		public bool IsInitialized
		{
			get
			{
				return _wrapper!=null && _wrapper.IsInitialized();
			}
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

		public void SendReceipt(IEventMessage message)
		{
		}

		public void InternalLog(Exception exception)
		{
			Logger.Error("Internal log error.", exception);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public void InvokeEventHandlers(EventMessage eventMessage, string[] routes)
		{
			foreach (var route in routes)
			{
				IList<SubscriptionWithHandler> reference;
				if (_subscriptionHandlers.TryGetValue(route, out reference))
				{
					var subscriptionList = new List<SubscriptionWithHandler>(reference);
					foreach (var subscriptionWithHandler in subscriptionList)
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
