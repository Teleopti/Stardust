using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Domain.MessageBroker
{
	public interface IMessageBrokerServer
	{
		string AddSubscription(Subscription subscription, string connectionId);
		void RemoveSubscription(string route, string connectionId);

		void AddMailbox(Subscription subscription);
		IEnumerable<Interfaces.MessageBroker.Message> PopMessages(string mailboxId);

		void NotifyClients(Interfaces.MessageBroker.Message message);
		void NotifyClientsMultiple(IEnumerable<Interfaces.MessageBroker.Message> notifications);

	}
}