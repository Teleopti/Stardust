using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.MessageBroker.Server
{
	public interface IMailboxRepository
	{
		void Persist(Mailbox mailbox);
		Mailbox Load(Guid id);
		IEnumerable<Mailbox> Load(string[] routes);
		void Purge();
	}
}