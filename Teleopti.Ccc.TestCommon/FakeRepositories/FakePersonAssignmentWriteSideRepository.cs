using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonAssignmentWriteSideRepository :
		IEnumerable<IPersonAssignment>,
		IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey>,
		IProxyForId<IPersonAssignment>
	{
		private readonly IList<IPersonAssignment> _entities = new List<IPersonAssignment>();

		public void Add (IPersonAssignment entity)
		{
			if (!entity.Id.HasValue)
				entity.SetId (Guid.NewGuid());
			_entities.Add (entity);
		}

		public void Remove (IPersonAssignment entity)
		{
			_entities.Remove (entity);
		}

		public IPersonAssignment Load (Guid id)
		{
			return _entities.Single (e => e.Id.Equals (id));
		}

		public IPersonAssignment LoadAggregate (PersonAssignmentKey id)
		{
			return _entities.FirstOrDefault (e => e.Person.Equals (id.Person) &&
			                                      e.Scenario.Equals (id.Scenario) &&
			                                      e.Date.Equals (id.Date));
		}

		public IEnumerator<IPersonAssignment> GetEnumerator()
		{
			return _entities.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _entities.GetEnumerator();
		}

	}
}