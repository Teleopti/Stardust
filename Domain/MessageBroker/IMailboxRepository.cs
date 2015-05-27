using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Domain.MessageBroker
{
	public interface IMailboxRepository
	{
		void Persist(Mailbox mailbox);
		Mailbox Load(Guid id);
		IEnumerable<Mailbox> Load(string[] routes);
	}

	public class EmtpyMailboxRepository : IMailboxRepository
	{
		public void Persist(Mailbox mailbox)
		{
		}

		public Mailbox Load(Guid id)
		{
			return new Mailbox();
		}

		public IEnumerable<Mailbox> Load(string[] routes)
		{
			return Enumerable.Empty<Mailbox>();
		}
	}
}