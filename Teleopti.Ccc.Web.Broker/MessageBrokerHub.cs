using SignalR.Hubs;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	[HubName("MessageBrokerHub")]
	public class MessageBrokerHub : Hub
	{
		public void AddSubscription(Subscription subscription)
		{
			var routes = subscription.Routes();
			foreach (var route in routes)
			{
				AddToGroup(route);
			}
		}

		public void RemoveSubscription(string route)
		{
			RemoveFromGroup(route);
		}

		public void NotifyClients(Notification notification)
		{
			var routes = notification.Routes();
			foreach (var route in routes)
			{
				Clients[route].onEventMessage(notification);
			}
		}
	}
}