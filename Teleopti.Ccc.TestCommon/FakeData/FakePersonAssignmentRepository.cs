using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	// feel free to continue implementing as see fit
	// im all for keeping it stupid (in the same manner as an .IgnoreArguments()) until smartness is required
	public class FakePersonAssignmentRepository : IPersonAssignmentRepository
	{
		private readonly IPersonAssignment _personAssignment;

		public FakePersonAssignmentRepository(IPersonAssignment personAssignment)
		{
			_personAssignment = personAssignment;
		}

		public void Add(IPersonAssignment entity)
		{
			throw new NotImplementedException();
		}

		public void Remove(IPersonAssignment entity)
		{
			throw new NotImplementedException();
		}

		public IPersonAssignment Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonAssignment> LoadAll()
		{
			throw new NotImplementedException();
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

		public ICollection<IPersonAssignment> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario)
		{
			return new Collection<IPersonAssignment>(new List<IPersonAssignment>(new[] {_personAssignment}));
		}

		public ICollection<IPersonAssignment> Find(DateOnlyPeriod period, IScenario scenario)
		{
			return new Collection<IPersonAssignment>(new List<IPersonAssignment>(new[] { _personAssignment }));
		}
	}
}