using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeWriteSideRepository<T> :
		IEnumerable<T>,
		IWriteSideRepository<T>,
		IProxyForId<T> where T : IAggregateRoot
	{
		private readonly IFakeStorage _storage;

		public FakeWriteSideRepository(IFakeStorage storage)
		{
			_storage = storage ?? new FakeStorageSimple();
		}

		public void Add(T entity)
		{
			if (!entity.Id.HasValue)
				entity.SetId(Guid.NewGuid());
			_storage.Add(entity);
		}

		public void Remove(T entity)
		{
			_storage.Remove(entity);
		}

		public T Load(Guid id)
		{
			return _storage.Get<T>(id);
		}

		public T LoadAggregate(Guid id)
		{
			return Load(id);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _storage.LoadAll<T>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _storage.LoadAll<T>().GetEnumerator();
		}
	}

}
