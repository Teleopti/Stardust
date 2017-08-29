using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeWriteSideRepository<T> :
		IEnumerable<T>,
		IWriteSideRepository<T>,
		IProxyForId<T> where T : IAggregateRoot
	{
		private readonly IList<T> _entities = new List<T>();

		public void Add(T entity)
		{
			if (!entity.Id.HasValue)
				entity.SetId(Guid.NewGuid());
			_entities.Add(entity);
		}

		public void Remove(T entity)
		{
			_entities.Remove(entity);
		}

		public T Load(Guid id)
		{
			return _entities.Single(e => e.Id.Equals(id));
		}

		public T LoadAggregate(Guid id)
		{
			return Load(id);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _entities.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _entities.GetEnumerator();
		}

	}
}
