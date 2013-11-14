using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	public class FakeWriteSideRepository<T> : IEnumerable<T>, IWriteSideRepository<T> where T : IAggregateRoot
	{
		protected readonly IList<T> Entities = new List<T>();

		public void Add(T entity)
		{
			if (!entity.Id.HasValue)
				entity.SetId(Guid.NewGuid());
			Entities.Add(entity);
		}

		public void Remove(T entity)
		{
			Entities.Remove(entity);
		}

		public T Load(Guid id)
		{
			return Entities.Single(e => e.Id.Equals(id));
		}

		public T LoadAggregate(Guid id)
		{
			return Load(id);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return Entities.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Entities.GetEnumerator();
		}

	}
}