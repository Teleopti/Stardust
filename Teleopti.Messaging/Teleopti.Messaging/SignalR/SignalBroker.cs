using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
using Teleopti.Messaging.SignalR.Wrappers;
using Subscription = Teleopti.Interfaces.MessageBroker.Subscription;

namespace Teleopti.Messaging.SignalR
{
	public class SignalBroker : IMessageBroker
	{
		private readonly ConcurrentDictionary<string, IList<SubscriptionWithHandler>> _subscriptionHandlers = new ConcurrentDictionary<string, IList<SubscriptionWithHandler>>();
		private ISignalConnectionHandler _connectionHandler;
		private ISignalBrokerCommands _signalBrokerCommands;
		private readonly object _wrapperLock = new object();
		private static readonly ILog Logger = LogManager.GetLogger(typeof (SignalBroker));

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
			if (e.Observed) return;
			Logger.Debug("An unobserved task failed.", e.Exception);
	        e.SetObserved();
        }

		private static bool ignoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
		{
			return true;
		}

	    private IMessageFilterManager FilterManager { get; set; }

		public void Dispose()
		{
			lock (_wrapperLock)
			{
				if (_connectionHandler == null) return;

				_connectionHandler.CloseConnection();
				_connectionHandler = null;
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
			var invoker = new HubProxyInvoker(callProxy);
			invoker.BeginInvoke(notificationList, callProxyCallback, invoker);
		}

		private void callProxy(IEnumerable<Notification> state)
		{
			lock (_wrapperLock)
			{
				if (_connectionHandler == null) return;

				_signalBrokerCommands.NotifyClients(state);
			}
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
				if (_connectionHandler == null) return;

				_signalBrokerCommands.AddSubscription(subscription);

				var route = subscription.Route();
				var handlers = _subscriptionHandlers.GetOrAdd(route, key => new List<SubscriptionWithHandler>());
				handlers.Add(new SubscriptionWithHandler {Handler = eventMessageHandler, Subscription = subscription});
			}
		}

		public void UnregisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			//currently does nothing due
			//if(subscriptionHandler.Count == 0 ...) below. Only second last foreach does anything. What? Don't know...
			//needs to be rewritten

			var handlersToRemove = new List<string>();

			lock (_wrapperLock)
			{
				if (_connectionHandler == null) return;

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
							// BUG? If count == 0 we never get here!
							//this is really, really wrong. Same as "if(false)" but we want that for now - otherwise bug 27055
							if (subscriptionHandler.Count == 0 && subscriptionWithHandler.Subscription != null)
							{
								var route = subscriptionWithHandler.Subscription.Route();
								_signalBrokerCommands.RemoveSubscription(route);
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

		public void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			registerEventSubscription(dataSource, businessUnitId, eventMessageHandler, referenceObjectId, referenceObjectType, null, domainObjectType, startDate, endDate);
		}

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

		public void StartMessageBroker()
		{
			StartMessageBroker(TimeSpan.FromSeconds(240));
		}
		
		public void StartMessageBroker(TimeSpan reconnectDelay)
		{
			Uri serverUrl;
			if (!Uri.TryCreate(ConnectionString, UriKind.Absolute, out serverUrl))
			{
				throw new BrokerNotInstantiatedException("The SignalBroker can only be used with a valid Uri!");
			}
			
			lock (_wrapperLock)
			{

				_connectionHandler = new SignalConnectionHandler(() => MakeHubConnection(serverUrl), null, reconnectDelay);

				_signalBrokerCommands = new SignalBrokerCommands(Logger, _connectionHandler);

				_connectionHandler.WithProxy(p =>
				{
					p.Subscribe("OnEventMessage").Received += obj =>
					{
						var d = obj[0].ToObject<Notification>();
						onNotification(d);
					};
				});

				_connectionHandler.StartConnection();
			}
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
				if (_connectionHandler != null)
				{
					_connectionHandler.CloseConnection();
				}
			}
		}

		public string ConnectionString { get; set; }

	    private bool IsTypeFilterApplied { get; set; }

		public bool IsInitialized
		{
			get
			{
				return _connectionHandler!=null && _connectionHandler.IsInitialized();
			}
		}

		private void InvokeEventHandlers(EventMessage eventMessage, IEnumerable<string> routes)
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
