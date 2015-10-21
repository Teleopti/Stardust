using System;
using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Domain.MessageBroker
{
	public interface IMailboxRepository
	{
		void Persist(Mailbox mailbox);
		Mailbox Load(Guid id);
		IEnumerable<Mailbox> Load(string[] routes);
		void Purge();
	}
}