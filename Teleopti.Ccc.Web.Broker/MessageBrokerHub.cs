using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SignalR.Hubs;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	[HubName("MessageBrokerHub")]
	public class MessageBrokerHub : Hub
	{
		private static readonly ConcurrentDictionary<string, IList<Subscription>> _subscriptionDictionary = new ConcurrentDictionary<string, IList<Subscription>>();
		private static readonly ConcurrentDictionary<string, DateTime> _pingDictionary = new ConcurrentDictionary<string, DateTime>();
		private static readonly object LockObject = new object();

		public Guid AddSubscription(Subscription subscription)
		{
			if (Guid.Empty == subscription.SubscriptionIdAsGuid())
			{
				subscription.SubscriptionId = Subscription.IdToString(Guid.NewGuid());
			}

			IList<Subscription> foundSubscriptions;
			if (!_subscriptionDictionary.TryGetValue(Context.ConnectionId, out foundSubscriptions))
			{
				foundSubscriptions = new List<Subscription>();
				_subscriptionDictionary.TryAdd(Context.ConnectionId, foundSubscriptions);
			}
			lock (LockObject)
			{
				foundSubscriptions.Add(subscription);
			}

			return subscription.SubscriptionIdAsGuid();
		}

		public void RemoveSubscription(Guid subscriptionId)
		{
			IList<Subscription> foundSubscriptions;
			if (_subscriptionDictionary.TryGetValue(Context.ConnectionId, out foundSubscriptions))
			{
				lock (LockObject)
				{
					var foundSubscriptionWithId = foundSubscriptions.FirstOrDefault(s => s.SubscriptionIdAsGuid() == subscriptionId);
					foundSubscriptions.Remove(foundSubscriptionWithId);
				}
			}
		}

		public void Ping()
		{
			var lastPing = DateTime.UtcNow;

			_pingDictionary.AddOrUpdate(Context.ConnectionId, lastPing, (key, oldPing) => lastPing);
			Caller.onPing(lastPing);
		}

		public void NotifyClients(Notification notification)
		{
			var notificationType = Type.GetType(notification.DomainType, false, true);
			var notificationReferenceType = notification.DomainReferenceType != null
												? Type.GetType(notification.DomainReferenceType, false, true)
												: null;

			IList<ClientWithSubscriptionDetail> clientSubscriptions;
			lock (LockObject)
			{
				clientSubscriptions = (from c in _subscriptionDictionary
									   from s in c.Value
									   where
										s != null && Type.GetType(s.DomainType, false, true).IsAssignableFrom(notificationType)
									   select new ClientWithSubscriptionDetail { ConnectionId = c.Key, SubscriptionDetail = s }).ToList();
			}

			foreach (var clientSubscription in clientSubscriptions)
			{
				if ((clientSubscription.SubscriptionDetail.DomainIdAsGuid() == Guid.Empty ||
					 clientSubscription.SubscriptionDetail.DomainIdAsGuid() == notification.DomainIdAsGuid()) &&
					(clientSubscription.SubscriptionDetail.DomainReferenceIdAsGuid() == Guid.Empty ||
					 clientSubscription.SubscriptionDetail.DomainReferenceIdAsGuid() == notification.DomainReferenceIdAsGuid()) &&
					(string.IsNullOrEmpty(clientSubscription.SubscriptionDetail.DomainReferenceType) ||
					 (notificationReferenceType != null &&
					  Type.GetType(clientSubscription.SubscriptionDetail.DomainReferenceType, false, true).IsAssignableFrom(
						notificationReferenceType))) &&
					((clientSubscription.SubscriptionDetail.LowerBoundaryAsDateTime() <= notification.EndDateAsDateTime() &&
					  clientSubscription.SubscriptionDetail.UpperBoundaryAsDateTime() >= notification.StartDateAsDateTime())))
				{
					Clients[clientSubscription.ConnectionId].onEventMessage(notification,
																		clientSubscription.SubscriptionDetail.SubscriptionIdAsGuid());
				}
			}
		}

		private class ClientWithSubscriptionDetail
		{
			public string ConnectionId { get; set; }
			public Subscription SubscriptionDetail { get; set; }
		}
	}
}