using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	public class MessageBrokerController
	{
		private readonly Func<IHubContext> _hubContext;

		public MessageBrokerController(Func<IHubContext> hubContext)
		{
			_hubContext = hubContext;
		}

		public void NotifyClients(Notification notification)
		{
			notification.Routes().ForEach(r =>
				_hubContext().Clients.Group(MessageBrokerServer.RouteToGroupName(r)).onEventMessage(notification, r)
				);
		}

		public void NotifyClientsMultiple(IEnumerable<Notification> notifications)
		{
			notifications.ForEach(NotifyClients);
		}
	}
}