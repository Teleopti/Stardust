using System.Collections.Generic;
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

		public string AddSubscription(Subscription subscription)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("New subscription from client {0} with route {1} (Id: {2}).", Context.ConnectionId, subscription.Route(), subscription.Route().GetHashCode());
			}
			var route = subscription.Route();
			Groups.Add(Context.ConnectionId, RouteToGroupName(route)).ContinueWith(t => Logger.InfoFormat("Added subscription {0}.", route));
			return route;
		}

		public void RemoveSubscription(string route)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Remove subscription from client {0} with route {1} (Id: {2}).", Context.ConnectionId, route, route.GetHashCode());
			}
			Groups.Remove(Context.ConnectionId, route);
		}

		public void NotifyClients(Notification notification)
		{
			var routes = notification.Routes();

			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("New notification from client {0} with routes {1} (Id's: {2}).", Context.ConnectionId, string.Join(", ", notification.Routes()), string.Join(", ", notification.Routes().Select(r => r.GetHashCode())));
			}

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

		public static string RouteToGroupName(string route)
		{
			//gethashcode won't work in 100% of the cases...
			return route.GetHashCode().ToString();
		}
	}
}