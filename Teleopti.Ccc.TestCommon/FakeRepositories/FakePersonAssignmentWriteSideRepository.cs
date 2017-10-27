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
		private readonly FakeStorage _storage;

		public FakePersonAssignmentWriteSideRepository(FakeStorage storage)
		{
			_storage = storage;
		}

		public void Add (IPersonAssignment entity)
		{
			if (!entity.Id.HasValue)
				entity.SetId (Guid.NewGuid());
			_storage.Add(entity);
		}

		public void Remove (IPersonAssignment entity)
		{
			_storage.Remove (entity);
		}

		public IPersonAssignment Load (Guid id)
		{
			return _storage.Get<IPersonAssignment>(id);
		}

		public IPersonAssignment LoadAggregate (PersonAssignmentKey id)
		{
			return _storage.LoadAll<IPersonAssignment>().FirstOrDefault (e => e.Person.Equals (id.Person) &&
			                                      e.Scenario.Equals (id.Scenario) &&
			                                      e.Date.Equals (id.Date));
		}

		public IEnumerator<IPersonAssignment> GetEnumerator()
		{
			return _storage.LoadAll<IPersonAssignment>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _storage.LoadAll<IPersonAssignment>().GetEnumerator();
		}
	}
}