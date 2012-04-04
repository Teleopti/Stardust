using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using SignalR.Hubs;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	[HubName("MessageBrokerHub")]
	public class MessageBrokerHub : Hub
	{
		private readonly static ILog Logger = LogManager.GetLogger(typeof (MessageBrokerHub));

		public void AddSubscription(Subscription subscription)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("New subscription from client {0} with route {1}.",Context.ConnectionId,subscription.Route());
			}
			AddToGroup(subscription.Route().GetHashCode().ToString());
		}

		public void RemoveSubscription(string route)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Remove subscription from client {0} with route {1}.", Context.ConnectionId, route);
			}
			RemoveFromGroup(route);
		}

		public void NotifyClients(Notification notification)
		{
			var routes = notification.Routes();

			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("New notification from client {0} with routes {1}.", Context.ConnectionId, string.Join(", ",notification.Routes()));
			}

			foreach (var route in routes)
			{
				Clients[route.GetHashCode().ToString()].onEventMessage(notification);
			}
		}
	}
}