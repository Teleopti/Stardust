using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeMailboxRepository : IMailboxRepository
	{
		private readonly INow _now;
		public readonly IList<Mailbox> Data = new List<Mailbox>();

		public FakeMailboxRepository(INow now)
		{
			_now = now;
		}

		public Mailbox PersistedLast
		{
			get { return Data.Last(); }
		}

		public void Persist(Mailbox mailbox)
		{
			var existing = Data.SingleOrDefault(x => x.Id.Equals(mailbox.Id));
			if (existing != null)
				Data.Remove(existing);
			Data.Add(mailbox.CopyBySerialization());
		}

		public Mailbox Load(Guid id)
		{
			return Data.SingleOrDefault(x => x.Id.Equals(id)).CopyBySerialization();
		}
		
		public IEnumerable<Mailbox> Load(string[] routes)
		{
			return (
				from m in Data
				where routes.Contains(m.Route)
				select m.CopyBySerialization()
				).ToArray();
		}

		public bool PurgeWasCalled;

		public void Purge()
		{
			PurgeWasCalled = true;
			var mailboxesToDelete = Data.Where(x => _now.UtcDateTime() > x.ExpiresAt).ToArray();
			mailboxesToDelete.ForEach(x => Data.Remove(x));
		}
	}
}