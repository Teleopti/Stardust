using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	// feel free to continue implementing as see fit
	// im all for keeping it stupid (in the same manner as an .IgnoreArguments()) until smartness is required
	public class FakePersonAssignmentRepository : IPersonAssignmentRepository
	{
		private readonly IList<IPersonAssignment> _personAssignments = new List<IPersonAssignment>();

		public FakePersonAssignmentRepository()
		{
			
		}

		public FakePersonAssignmentRepository(IPersonAssignment personAssignment)
		{
			_personAssignments.Add(personAssignment);
		}

		public void Add(IPersonAssignment entity)
		{
			_personAssignments.Add(entity);
		}

		public void Remove(IPersonAssignment entity)
		{
			_personAssignments.Remove(entity);
		}

		public IPersonAssignment Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonAssignment> LoadAll()
		{
			return _personAssignments;
		}

		public IPersonAssignment Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IPersonAssignment> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public IPersonAssignment LoadAggregate(Guid id)
		{
			throw new NotImplementedException();
		}

		public IPersonAssignment LoadAggregate(PersonAssignmentKey id)
		{
			throw new NotImplementedException();
		}

		public ICollection<IPersonAssignment> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario)
		{
			return new Collection<IPersonAssignment>(_personAssignments);
		}

		public ICollection<IPersonAssignment> Find(DateOnlyPeriod period, IScenario scenario)
		{
			return new Collection<IPersonAssignment>(_personAssignments);
		}

		public IEnumerable<DateScenarioPersonId> FetchDatabaseVersions(DateOnlyPeriod period, IScenario scenario, IPerson person)
		{
			throw new NotImplementedException();
		}
	}
}