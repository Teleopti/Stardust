using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	[CLSCompliant(false)]
	public class MessageBrokerServer : IMessageBrokerServer
	{
		private readonly IActionScheduler _actionScheduler;
		private readonly ISignalR _signalR;
		public ILog Logger = LogManager.GetLogger(typeof(MessageBrokerHub));

		public MessageBrokerServer(IActionScheduler actionScheduler, ISignalR signalR)
		{
			_actionScheduler = actionScheduler;
			_signalR = signalR;
		}

		public void NotifyClients(Notification notification)
		{
			_signalR.CallOnEventMessage(null, null, notification);
		}

		public void NotifyClients(ISignalR signalR, string connectionId, Notification notification)
		{
			var routes = notification.Routes();

			if (Logger.IsDebugEnabled)
				Logger.DebugFormat("New notification from client {0} with (DomainUpdateType: {1}) (Routes: {2}) (Id: {3}).",
					connectionId, notification.DomainUpdateType, string.Join(", ", routes),
					string.Join(", ", routes.Select(RouteToGroupName)));

			foreach (var route in routes)
			{
				var r = route;
				_actionScheduler.Do(() => signalR.CallOnEventMessage(RouteToGroupName(r), r, notification));
			}
		}

		public void NotifyClientsMultiple(ISignalR clients, string connectionId, IEnumerable<Notification> notifications)
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