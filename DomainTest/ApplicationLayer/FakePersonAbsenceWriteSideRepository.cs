using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	public class FakePersonAbsenceWriteSideRepository :
		IEnumerable<IPersonAbsence>,
		IWriteSideRepositoryTypedId<IPersonAbsence, Guid>,
		IProxyForId<IPersonAbsence>
	{
		private readonly IList<IPersonAbsence> _entities = new List<IPersonAbsence>();

		public void Add(IPersonAbsence entity)
		{
			if (!entity.Id.HasValue)
				entity.SetId(Guid.NewGuid());
			_entities.Add(entity);
		}

		public void Remove(IPersonAbsence entity)
		{
			_entities.Remove(entity);
		}

		public IPersonAbsence Load(Guid id)
		{
			return _entities.Single(e => e.Id.Equals(id));
		}

		public IPersonAbsence LoadAggregate(Guid id)
		{
			return _entities.FirstOrDefault(e => e.Id.Equals(id));
		}

		public IEnumerator<IPersonAbsence> GetEnumerator()
		{
			return _entities.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _entities.GetEnumerator();
		}

	}
}