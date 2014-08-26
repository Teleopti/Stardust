using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNet.SignalR.Hubs;
using log4net;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	[HubName("MessageBrokerHub")]
	public class MessageBrokerHub : TestableHub
	{
		public ILog Logger = LogManager.GetLogger(typeof(MessageBrokerHub));

		private readonly IActionScheduler _actionScheduler;
		private readonly IBeforeSubscribe _beforeSubscribe;

		public MessageBrokerHub(IActionScheduler actionScheduler, IBeforeSubscribe beforeSubscribe)
		{
			_actionScheduler = actionScheduler;
			_beforeSubscribe = beforeSubscribe;
		}

		public string AddSubscription(Subscription subscription)
		{
			_beforeSubscribe.Invoke(subscription);

			var route = subscription.Route();
			
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("New subscription from client {0} with route {1} (Id: {2}).", Context.ConnectionId,
								   route, RouteToGroupName(route));
			}
			

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
				var r = route;
				_actionScheduler.Do(() => Clients.Group(RouteToGroupName(r)).onEventMessage(notification, r));
			}
		}

		public void NotifyClientsMultiple(IEnumerable<Notification> notifications)
		{
			foreach (var notification in notifications)
			{
				NotifyClients(notification);
			}
		}

		public void Ping()
		{
			Clients.Caller.Pong();
		}

		public void PingWithId(double id)
		{
			Clients.Caller.Pong(id);
		}

		public void Ping(int expectedNumberOfSentMessages)
		{
			for (var i = 0; i < expectedNumberOfSentMessages; i++)
			{
				Ping();
			}
		}

		public void Ping(int expectedNumberOfSentMessages, int messagesPerSecond)
		{
			// FIXME to be able to change the throttle
			// System.Web.Configuration.WebConfigurationManager.AppSettings["MessagesPerSecond"] = messagesPerSecond.ToString();
			Ping(expectedNumberOfSentMessages);
		}
	}
}