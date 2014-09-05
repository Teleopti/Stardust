using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	public class MessageBrokerController
	{
		private readonly Func<IHubContext> _hubContext;
		private readonly MessageBrokerServer _server;

		public MessageBrokerController(Func<IHubContext> hubContext)
		{
			_server = new MessageBrokerServer(new ActionImmediate());
			_hubContext = hubContext;
		}

		public void NotifyClients(Notification notification)
		{
			_server.NotifyClients(_hubContext().Clients, "POST", notification);
		}

		public void NotifyClientsMultiple(IEnumerable<Notification> notifications)
		{
			_server.NotifyClientsMultiple(_hubContext().Clients, "POST", notifications);
		}
	}
}