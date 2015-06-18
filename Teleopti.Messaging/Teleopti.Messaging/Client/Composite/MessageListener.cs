using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;

namespace Teleopti.Messaging.Client.Composite
{
	public class MessageListener : IMessageListener
	{
		private readonly ISignalRClient _client;
		private readonly IList<subscriptionCallback> _subscriptions = new List<subscriptionCallback>();

		private class subscriptionCallback
		{
			public Subscription Subscription { get; set; }
			public EventHandler<EventMessageArgs> Callback { get; set; }
		}

		public MessageListener(ISignalRClient client)
		{
			_client = client;
		}

		public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			_subscriptions.Add(new subscriptionCallback
			{
				Subscription = subscription,
				Callback = eventMessageHandler
			});

			_client.Call("AddSubscription", subscription);
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
				_client.Call("AddSubscription", subscription.Subscription);
		}

		public void OnNotification(Message d)
		{
			var message = new EventMessage
			{
				InterfaceType = Type.GetType(d.DomainQualifiedType, false, true),
				DomainObjectType = d.DomainType,
				DomainObjectId = d.DomainIdAsGuid(),
				ModuleId = d.ModuleIdAsGuid(),
				ReferenceObjectId = d.DomainReferenceIdAsGuid(),
				EventStartDate = d.StartDateAsDateTime(),
				EventEndDate = d.EndDateAsDateTime(),
				DomainUpdateType = d.DomainUpdateTypeAsDomainUpdateType()
			};

			// assuming all binary data sent is base 64? 
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