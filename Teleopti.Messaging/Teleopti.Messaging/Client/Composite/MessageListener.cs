using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Messaging.Client.Composite
{
	public class MessageListener : IMessageListener
	{
		private readonly ISignalRClient _client;
		private readonly IList<SubscriptionCallback> _subscriptions = new List<SubscriptionCallback>();

		private class SubscriptionCallback
		{
			public Subscription Subscription { get; set; }
			public EventHandler<EventMessageArgs> Callback { get; set; }
		}

		public MessageListener(ISignalRClient client)
		{
			_client = client;
		}

		public void RegisterSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType)
		{
			addSubscription(dataSource, businessUnitId, eventMessageHandler, null, null, null, domainObjectType, Consts.MinDate, Consts.MaxDate);
		}

		public void RegisterSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Type domainObjectType)
		{
			addSubscription(dataSource, businessUnitId, eventMessageHandler, referenceObjectId, referenceObjectType, null, domainObjectType, Consts.MinDate, Consts.MaxDate);
		}

		public void RegisterSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			addSubscription(dataSource, businessUnitId, eventMessageHandler, null, null, null, domainObjectType, startDate, endDate);
		}

		public void RegisterSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			addSubscription(dataSource, businessUnitId, eventMessageHandler, null, null, domainObjectId, domainObjectType, startDate, endDate);
		}

		public void RegisterSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			addSubscription(dataSource, businessUnitId, eventMessageHandler, referenceObjectId, referenceObjectType, null, domainObjectType, startDate, endDate);
		}

		private void addSubscription(
			string datasource,
			Guid businessUnitId,
			EventHandler<EventMessageArgs> eventMessageHandler,
			Guid? referenceObjectId,
			Type referenceObjectType,
			Guid? domainObjectId,
			Type domainObjectType,
			DateTime startDate,
			DateTime endDate)
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

			_subscriptions.Add(new SubscriptionCallback
			{
				Subscription = subscription,
				Callback = eventMessageHandler
			});

			addSubscription(subscription);
		}

		public void UnregisterSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			// cleanup refactoring, but keeping the same buggy behavior: does not remove subscription from the server.
			// should also do this somewhere, when there are no local routes left:
			//_signalBrokerCommands.RemoveSubscription(route);
			// if you want more information check hg history

			var toRemove = (from s in _subscriptions
				where s.Callback == eventMessageHandler
				select s).ToList();

			toRemove.ForEach(s => _subscriptions.Remove(s));
		}

		public void ReregisterSubscriptions()
		{
			foreach (var subscription in _subscriptions)
				addSubscription(subscription.Subscription);
		}

		private void addSubscription(Subscription subscription)
		{
			_client.Call("AddSubscription", subscription);
		}

		public void OnNotification(Notification d)
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

		private void InvokeEventHandlers(EventMessage eventMessage, IEnumerable<string> routes)
		{
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
}