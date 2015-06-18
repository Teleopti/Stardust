using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;

namespace Teleopti.Messaging.Client
{
	public class EventHandlers
	{
		private readonly IList<subscriptionCallback> _subscriptions = new List<subscriptionCallback>();

		private class subscriptionCallback
		{
			public Subscription Subscription { get; set; }
			public EventHandler<EventMessageArgs> Callback { get; set; }
		}

		public void Add(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			_subscriptions.Add(new subscriptionCallback
			{
				Subscription = subscription,
				Callback = eventMessageHandler
			});
		}

		public void Remove(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			var toRemove = (from s in _subscriptions
				where s.Callback == eventMessageHandler
				select s).ToList();
			toRemove.ForEach(s => _subscriptions.Remove(s));
		}

		public void ForAll(Action<Subscription> action)
		{
			_subscriptions
				.Select(x => x.Subscription)
				.ForEach(action);
		}

		public void CallHandlers(Message message)
		{
			var eventMessage = new EventMessage
			{
				InterfaceType = Type.GetType(message.DomainQualifiedType, false, true),
				DomainObjectType = message.DomainType,
				DomainObjectId = message.DomainIdAsGuid(),
				ModuleId = message.ModuleIdAsGuid(),
				ReferenceObjectId = message.DomainReferenceIdAsGuid(),
				EventStartDate = message.StartDateAsDateTime(),
				EventEndDate = message.EndDateAsDateTime(),
				DomainUpdateType = message.DomainUpdateTypeAsDomainUpdateType()
			};

			// assuming all binary data sent is base 64?
			// OHHHHHH NOOOOOOOO!!!!!
			var domainObject = message.BinaryData;
			if (!string.IsNullOrEmpty(domainObject))
			{
				eventMessage.DomainObject = Convert.FromBase64String(domainObject);
			}

			var matchingHandlers = from s in _subscriptions
				from r in message.Routes()
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