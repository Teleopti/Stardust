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
			var routes = notification.Routes();

			if (Logger.IsDebugEnabled)
				Logger.DebugFormat("New notification from client with (DomainUpdateType: {0}) (Routes: {1}) (Id: {2}).",
					notification.DomainUpdateType, string.Join(", ", routes),
					string.Join(", ", routes.Select(RouteToGroupName)));

			foreach (var route in routes)
			{
				var r = route;
				_actionScheduler.Do(() => _signalR.CallOnEventMessage(RouteToGroupName(r), r, notification));
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