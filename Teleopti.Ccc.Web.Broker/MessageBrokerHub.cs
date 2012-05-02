using System.Linq;
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
				Logger.DebugFormat("New subscription from client {0} with route {1} (Id: {2}).",Context.ConnectionId,subscription.Route(),subscription.Route().GetHashCode());
			}
			Groups.Add(Context.ConnectionId, subscription.Route().GetHashCode().ToString()).ContinueWith(t => Logger.InfoFormat("Added subscription {0}.",subscription.Route()));
		}

		public void RemoveSubscription(string route)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Remove subscription from client {0} with route {1} (Id: {2}).", Context.ConnectionId, route,route.GetHashCode());
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
				Clients[route.GetHashCode().ToString()].onEventMessage(notification);
			}
		}
	}
}