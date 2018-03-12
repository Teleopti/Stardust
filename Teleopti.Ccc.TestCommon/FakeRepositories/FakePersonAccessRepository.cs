using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonAccessRepository : IRepository<IPersonAccess>
	{
		private readonly IFakeStorage _storage;

		public FakePersonAccessRepository(IFakeStorage storage)
		{
			_storage = storage;
		}

		public void Add(IPersonAccess root)
		{
			_storage.Add(root);
		}

		public IPersonAccess Get(Guid id)
		{
			return _storage.Get<IPersonAccess>(id);
		}

		public IPersonAccess Load(Guid id)
		{
			return _storage.Get<IPersonAccess>(id);
		}

		public IEnumerable<IPersonAccess> LoadAll()
		{
			return _storage.LoadAll<IPersonAccess>();
		}

		public void Remove(IPersonAccess root)
		{
			_storage.Remove(root);
		}
	}
}
