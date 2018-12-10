using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.MessageBroker.Server
{
	public interface IMessageBrokerServer
	{
		string AddSubscription(Subscription subscription, string connectionId);
		void RemoveSubscription(string route, string connectionId);

		IEnumerable<Message> PopMessages(string route, string mailboxId);

		void NotifyClients(Message message);
		void NotifyClientsMultiple(IEnumerable<Message> messages);

	}
}