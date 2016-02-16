using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Messaging.Client
{
	public class EventHandlers
	{
		private readonly IList<SubscriptionInfo> _subscriptions = new List<SubscriptionInfo>();

		public class SubscriptionInfo
		{
			public Subscription Subscription { get; set; }
			public EventHandler<EventMessageArgs> Callback { get; set; }
		}

		public void Add(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			_subscriptions.Add(new SubscriptionInfo
			{
				Subscription = subscription,
				Callback = eventMessageHandler,
			});
		}

		public void Remove(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			var toRemove = (from s in _subscriptions
				where s.Callback == eventMessageHandler
				select s).ToList();
			toRemove.ForEach(s => _subscriptions.Remove(s));
		}

		public IEnumerable<SubscriptionInfo> All()
		{
			return _subscriptions.ToArray();
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
				DomainUpdateType = message.DomainUpdateTypeAsDomainUpdateType(),
			};

			var matchingHandlers = from s in _subscriptions
				from r in message.Routes()
				let route = s.Subscription.Route()
				where
					route == r &&
					s.Subscription.LowerBoundaryAsDateTime() <= eventMessage.EventEndDate &&
					s.Subscription.UpperBoundaryAsDateTime() >= eventMessage.EventStartDate
				select s;

			foreach (var subscription in matchingHandlers)
			{
				var e = new EventMessageArgs(eventMessage) {InternalMessage = message};
				if (subscription.Subscription.Base64BinaryData && !string.IsNullOrEmpty(e.InternalMessage.BinaryData))
				{
					e.Message.DomainObject = Convert.FromBase64String(e.InternalMessage.BinaryData);
					e.InternalMessage.BinaryData = null;
				}
				subscription.Callback(this, e);
			}


		}

	}
}