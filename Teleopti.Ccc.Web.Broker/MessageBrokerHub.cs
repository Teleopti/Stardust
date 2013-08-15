using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using log4net;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	[HubName("MessageBrokerHub")]
	public class MessageBrokerHub : TestableHub
	{
		public ILog Logger = LogManager.GetLogger(typeof(MessageBrokerHub));

		public string AddSubscription(Subscription subscription)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("New subscription from client {0} with route {1} (Id: {2}).", Context.ConnectionId,
								   subscription.Route(), RouteToGroupName(subscription.Route()));
			}
			var route = subscription.Route();
			Groups.Add(Context.ConnectionId, RouteToGroupName(route))
				  .ContinueWith(t => Logger.InfoFormat("Added subscription {0}.", route));

			return route;
		}

		public void RemoveSubscription(string route)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Remove subscription from client {0} with route {1} (Id: {2}).", Context.ConnectionId, route,
								   route.GetHashCode());
			}
			Groups.Remove(Context.ConnectionId, route);
		}

		public static string RouteToGroupName(string route)
		{
			//gethashcode won't work in 100% of the cases...
			UInt64 hashedValue = 3074457345618258791ul;
			for (int i = 0; i < route.Length; i++)
			{
				hashedValue += route[i];
				hashedValue *= 3074457345618258799ul;
			}
			return hashedValue.GetHashCode().ToString(CultureInfo.InvariantCulture);
		}

		public void NotifyClients(Notification notification)
		{
			var routes = notification.Routes();

			if (Logger.IsDebugEnabled)
				Logger.DebugFormat("New notification from client {0} with (DomainUpdateType: {1}) (Routes: {2}) (Id: {3}).",
								   Context.ConnectionId, notification.DomainUpdateType, string.Join(", ", routes),
								   string.Join(", ", routes.Select(RouteToGroupName)));

			foreach (var route in routes)
			{
				Clients.Group(RouteToGroupName(route)).onEventMessage(notification, route);
			}
		}

		public void NotifyClientsMultiple(IEnumerable<Notification> notifications)
		{
			foreach (var notification in notifications)
			{
				NotifyClients(notification);
			}
		}
	}
}

//	[HubName("MessageBrokerHub")]
//	public class MessageBrokerHub : TestableHub
//	{
//		private readonly MessageBrokerOnIntervalSender _messageBrokerOnIntervalSender;
//		public ILog Logger = LogManager.GetLogger(typeof (MessageBrokerHub));

//		public MessageBrokerHub() : this(MessageBrokerOnIntervalSender.Instance) {}

//		public MessageBrokerHub(MessageBrokerOnIntervalSender messageBrokerOnIntervalSender)
//		{
//			_messageBrokerOnIntervalSender = messageBrokerOnIntervalSender;
//		}

//		public IEnumerable<Notification> GetAllNotifications()
//		{
//			return _messageBrokerOnIntervalSender.GetAllNotifications();
//		}

//		public string AddSubscription(Subscription subscription)
//		{
//			if (Logger.IsDebugEnabled)
//			{
//				Logger.DebugFormat("New subscription from client {0} with route {1} (Id: {2}).", Context.ConnectionId,
//								   subscription.Route(), _messageBrokerOnIntervalSender.RouteToGroupName(subscription.Route()));
//			}
//			var route = subscription.Route();
//			Groups.Add(Context.ConnectionId, _messageBrokerOnIntervalSender.RouteToGroupName(route))
//				  .ContinueWith(t => Logger.InfoFormat("Added subscription {0}.", route));

//			return route;
//		}

//		public void RemoveSubscription(string route)
//		{
//			if (Logger.IsDebugEnabled)
//			{
//				Logger.DebugFormat("Remove subscription from client {0} with route {1} (Id: {2}).", Context.ConnectionId, route,
//								   route.GetHashCode());
//			}
//			Groups.Remove(Context.ConnectionId, route);
//		}

//		public void NotifyClientsMultiple(IEnumerable<Notification> notifications)
//		{
//			foreach (var notification in notifications)
//			{
//				NotifyClients(notification);
//			}
//		}

//		public void NotifyClients(Notification notification)
//		{
//			_messageBrokerOnIntervalSender.AddNotification(notification);
//			if (Logger.IsDebugEnabled)
//				Logger.DebugFormat("New notification from client {0} with (DomainUpdateType: {1} and DomainType: {2}).",
//								   Context.ConnectionId, notification.DomainUpdateType, notification.DomainType);
//		}
//	}

//	public class MessageBrokerOnIntervalSender
//	{
//		public ILog Logger = LogManager.GetLogger(typeof(MessageBrokerOnIntervalSender));

//		// Singleton instance
//		private readonly static Lazy<MessageBrokerOnIntervalSender> _instance =
//			new Lazy<MessageBrokerOnIntervalSender>(() => new MessageBrokerOnIntervalSender(GlobalHost.ConnectionManager.GetHubContext<MessageBrokerHub>().Clients));
		
//		private readonly ConcurrentDictionary<long, Notification> _notifications = new ConcurrentDictionary<long, Notification>();

//		private readonly object _updateNotificationLock = new object();

//		private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(250);
		
//		private readonly Timer _timer;
//		private volatile bool _updatingNotifications = false;

//		private static long _nextId;

//		private MessageBrokerOnIntervalSender(IHubConnectionContext clients)
//		{
//			Clients = clients;

//			_timer = new Timer(updateNotifications, null, _updateInterval, _updateInterval);
//		}

//		public static MessageBrokerOnIntervalSender Instance
//		{
//			get { return _instance.Value; }
//		}

//		private IHubConnectionContext Clients { get; set; }

//		public void AddNotification(Notification notification)
//		{
//			if (_notifications.TryAdd(nextId(), notification))
//			{
//				if (Logger.IsDebugEnabled && (notification.DomainType == "IPersonAbsence" || notification.DomainType == "IPersonScheduleDayReadModel"))
//					Logger.DebugFormat("Notification added with (DomainUpdateType: {0} and DomainType: {1}).",
//									   notification.DomainUpdateType, notification.DomainType);
//			}
//			else
//			{
//				if (Logger.IsDebugEnabled && (notification.DomainType == "IPersonAbsence" || notification.DomainType == "IPersonScheduleDayReadModel"))
//					Logger.DebugFormat("Notification NOT added with (DomainUpdateType: {0} and DomainType: {1}).",
//									   notification.DomainUpdateType, notification.DomainType);
//			}
//		}

//		private void removeNotification(long key)
//		{
//			Notification value1;
			
//			if (_notifications.TryRemove(key, out value1))
//			{
//				if (Logger.IsDebugEnabled && (value1.DomainType == "IPersonAbsence" || value1.DomainType == "IPersonScheduleDayReadModel"))
//					Logger.DebugFormat("Notification removed with (DomainUpdateType: {0} and DomainType: {1}).",
//									   value1.DomainUpdateType, value1.DomainType);
//			}
//			else
//			{
//				if (Logger.IsDebugEnabled)
//					Logger.DebugFormat("Notification NOT removed");
//			}
//		}

//		private static long nextId()
//		{
//			return Interlocked.Increment(ref _nextId);
//		}

//		public IEnumerable<Notification> GetAllNotifications()
//		{
//			return _notifications.Values;
//		}

//		private void updateNotifications(object state)
//		{
//			lock (_updateNotificationLock)
//			{
//				if (!_updatingNotifications)
//				{
//					_updatingNotifications = true;

//					foreach (var notification in _notifications)
//					{
//						broadcastNotification(notification.Value);
//						removeNotification(notification.Key);
//					}

//					_updatingNotifications = false;
//				}
//			}
//		}

//		public string RouteToGroupName(string route)
//		{
//			//gethashcode won't work in 100% of the cases...
//			UInt64 hashedValue = 3074457345618258791ul;
//			for (int i = 0; i < route.Length; i++)
//			{
//				hashedValue += route[i];
//				hashedValue *= 3074457345618258799ul;
//			}
//			return hashedValue.GetHashCode().ToString(CultureInfo.InvariantCulture);
//		}

//		private void broadcastNotification(Notification notification)
//		{
//			var routes = notification.Routes();

//			foreach (var route in routes)
//			{
//				if (Logger.IsDebugEnabled && (notification.DomainType == "IPersonAbsence" || notification.DomainType == "IPersonScheduleDayReadModel"))
//					Logger.DebugFormat("Notification broadcasted with (DomainUpdateType: {0} and DomainType: {1} and Route: {2}).",
//					   notification.DomainUpdateType, notification.DomainType, routes[routes.Length - 1]);

//				Clients.Group(RouteToGroupName(route)).onEventMessage(notification, route);
//				Thread.Sleep(45);
//			}
//		}
//	}
//}


	