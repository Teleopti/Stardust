using System;
using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	[CLSCompliant(false)]
	public interface IMessageBrokerServer
	{
		void NotifyClients(Notification notification);
		void NotifyClients(ISignalR signalR, string connectionId, Notification notification);
		void NotifyClientsMultiple(ISignalR clients, string connectionId, IEnumerable<Notification> notifications);
	}
}