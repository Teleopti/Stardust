using System.Collections.Generic;
using System.Web.Mvc;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	public class MessageBrokerController : Controller
	{
		private readonly IMessageBrokerServer _server;

		public MessageBrokerController(IMessageBrokerServer server)
		{
			_server = server;
		}

		public void NotifyClients(Notification notification)
		{
			_server.NotifyClients(notification);
		}

		public void NotifyClientsMultiple(IEnumerable<Notification> notifications)
		{
			_server.NotifyClientsMultiple(notifications);
		}
	}
}