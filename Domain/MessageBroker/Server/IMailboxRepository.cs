using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.MessageBroker.Server
{
	public interface IMailboxRepository
	{
		void Add(Mailbox mailbox);
		Mailbox Load(Guid id);
		IEnumerable<Message> PopMessages(Guid id, DateTime? expiredAt);
		void AddMessage(Message message);
		void Purge();
	}
}