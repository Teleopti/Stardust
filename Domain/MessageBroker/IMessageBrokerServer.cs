﻿using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Domain.MessageBroker
{
	public interface IMessageBrokerServer
	{
		string AddSubscription(Subscription subscription, string connectionId);
		void RemoveSubscription(string route, string connectionId);

		IEnumerable<Message> PopMessages(string route, string mailboxId);

		void NotifyClients(Message message);
		void NotifyClientsMultiple(IEnumerable<Message> notifications);

	}
}