using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePushMessageRepository : IPushMessageRepository
	{
		private readonly FakeStorage storage;

		public FakePushMessageRepository()
		{
			this.storage = new FakeStorage();
		}

		public void Add(IPushMessage root)
		{
			storage.Add(root);
		}

		public void Remove(IPushMessage pushMessage)
		{
			storage.Remove(pushMessage);
		}

		public IPushMessage Get(Guid id)
		{
			return storage.Get<IPushMessage>(id);
		}

		public IPushMessage Load(Guid id)
		{
			return Get(id);
		}

		public IEnumerable<IPushMessage> LoadAll()
		{
			return storage.LoadAll<IPushMessage>();
		}

		public ICollection<IPushMessage> Find(IPerson sender, PagingDetail pagingDetail)
		{
			throw new NotImplementedException();
		}
	}
}
