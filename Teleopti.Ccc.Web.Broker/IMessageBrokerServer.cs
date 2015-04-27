using System;
using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	[CLSCompliant(false)]
	public interface IMessageBrokerServer
	{
		void NotifyClients(Notification notification);
		void NotifyClientsMultiple(IEnumerable<Notification> notifications);
	}
}