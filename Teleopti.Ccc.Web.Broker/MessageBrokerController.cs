using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	public class MessageBrokerController : Controller
	{
		public Func<IHubContext> HubContext = () => GlobalHost.ConnectionManager.GetHubContext<MessageBrokerHub>();

		private readonly MessageBrokerServer _server;

		public MessageBrokerController()
		{
			_server = new MessageBrokerServer(new ActionImmediate());
		}

		public void NotifyClients(Notification notification)
		{
			_server.NotifyClients(HubContext().Clients, "POST", notification);
		}

		public void NotifyClientsMultiple(IEnumerable<Notification> notifications)
		{
			_server.NotifyClientsMultiple(HubContext().Clients, "POST", notifications);
		}
	}
}