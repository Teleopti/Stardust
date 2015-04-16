using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	public class MessageBrokerServer
	{
		private readonly IActionScheduler _actionScheduler;
		public ILog Logger = LogManager.GetLogger(typeof(MessageBrokerHub));

		public MessageBrokerServer(IActionScheduler actionScheduler)
		{
			_actionScheduler = actionScheduler;
		}

		public void NotifyClients(dynamic clients, string connectionId, Notification notification)
		{
			var routes = notification.Routes();

			if (Logger.IsDebugEnabled)
				Logger.DebugFormat("New notification from client {0} with (DomainUpdateType: {1}) (Routes: {2}) (Id: {3}).",
					connectionId, notification.DomainUpdateType, string.Join(", ", routes),
					string.Join(", ", routes.Select(RouteToGroupName)));

			foreach (var route in routes)
			{
				var r = route;
				_actionScheduler.Do(() => clients.Group(RouteToGroupName(r)).onEventMessage(notification, r));
			}
		}

		public void NotifyClientsMultiple(dynamic clients, string connectionId, IEnumerable<Notification> notifications)
		{
			foreach (var notification in notifications)
			{
				NotifyClients(clients, connectionId, notification);
			}
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

	}

}