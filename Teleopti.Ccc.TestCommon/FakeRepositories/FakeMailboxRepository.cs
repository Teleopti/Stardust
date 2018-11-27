using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeMailboxRepository : IMailboxRepository
	{
		private readonly INow _now;
		private readonly IList<MailboxWithMessages> _data = new List<MailboxWithMessages>();

		public class MailboxWithMessages : Mailbox
		{
			public readonly IList<Message> Messages = new List<Message>();
		}

		public FakeMailboxRepository(INow now)
		{
			_now = now;
		}

		public bool PurgeWasCalled;

		public IEnumerable<MailboxWithMessages> Data => _data.ToArray();
		public Mailbox PersistedLast => Data.Last();

		public void Add(Mailbox mailbox)
		{
			_data.Add(new MailboxWithMessages
			{
				Id = mailbox.Id,
				Route = mailbox.Route,
				ExpiresAt = mailbox.ExpiresAt
			});
		}

		public Mailbox Load(Guid id)
		{
			return _data.SingleOrDefault(x => x.Id.Equals(id))
				.CopyBySerialization<MailboxWithMessages, Mailbox>();
		}

		public IEnumerable<Message> PopMessages(Guid id, DateTime? expiredAt)
		{
			var mailbox = _data.SingleOrDefault(x => x.Id.Equals(id));
			var messages = mailbox.Messages.ToArray();
			mailbox.Messages.Clear();
			mailbox.ExpiresAt = expiredAt ?? mailbox.ExpiresAt;
			return messages;
		}

		public void AddMessage(Message message)
		{
			_data.Where(x => message.Routes().Contains(x.Route))
				.ForEach(x => x.Messages.Add(message));
		}
		
		public void Purge()
		{
			PurgeWasCalled = true;
			var mailboxesToDelete = _data.Where(x => _now.UtcDateTime() > x.ExpiresAt).ToArray();
			mailboxesToDelete.ForEach(x => _data.Remove(x));
		}
	}
}