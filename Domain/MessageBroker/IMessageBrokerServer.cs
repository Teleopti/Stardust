
using System;
using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Domain.MessageBroker
{
	[CLSCompliant(false)]
	public interface IMessageBrokerServer
	{
		string AddSubscription(Subscription subscription, string connectionId);
		void NotifyClients(Interfaces.MessageBroker.Notification notification);
		void NotifyClientsMultiple(IEnumerable<Interfaces.MessageBroker.Notification> notifications);
	}
}