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
	}

	public class EmptyMailboxRepository : IMailboxRepository
	{
		public void Persist(Mailbox mailbox)
		{
		}

		public Mailbox Load(Guid id)
		{
			return null;
		}

		public IEnumerable<Mailbox> Load(string[] routes)
		{
			return new List<Mailbox>();
		}
	}
}