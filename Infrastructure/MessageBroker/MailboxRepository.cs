using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.MessageBroker;

namespace Teleopti.Ccc.Infrastructure.MessageBroker
{
	public class MailboxRepository : IMailboxRepository
	{
		public void Persist(Mailbox mailbox)
		{
		}

		public Mailbox Get(Guid id)
		{
			return null;
		}

		public IEnumerable<Mailbox> Get(string route)
		{
			return new Mailbox[] {};
		}
	}
}