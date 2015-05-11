using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.MessageBroker
{
	[CLSCompliant(false)]
	public interface IMessageBrokerServer
	{
		void NotifyClients(Interfaces.MessageBroker.Notification notification);
		void NotifyClientsMultiple(IEnumerable<Interfaces.MessageBroker.Notification> notifications);
	}
}