using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.MessageBroker;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeMailboxRepository : IMailboxRepository
	{
		public readonly IList<Mailbox> Data = new List<Mailbox>();

		public Mailbox PersistedLast
		{
			get { return Data.Last(); }
		}

		public void Persist(Mailbox mailbox)
		{
			var existing = Data.SingleOrDefault(x => x.Id.Equals(mailbox.Id));
			if (existing != null)
				Data.Remove(existing);
			Data.Add(JsonConvert.DeserializeObject<Mailbox>(JsonConvert.SerializeObject(mailbox)));
		}

		public Mailbox Get(Guid id)
		{
			return Data
				.SingleOrDefault(x => x.Id.Equals(id));
		}

		public IEnumerable<Mailbox> Get(string route)
		{
			return Data
				.Where(x => x.Route.Equals(route))
				.ToArray()
				;
		}
	}
}