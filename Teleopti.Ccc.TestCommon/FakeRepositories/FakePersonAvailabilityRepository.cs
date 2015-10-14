using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonAvailabilityRepository : IPersonAvailabilityRepository
	{
		public void Add(IPersonAvailability root)
		{
			throw new NotImplementedException();
		}

		public void Remove(IPersonAvailability root)
		{
			throw new NotImplementedException();
		}

		public IPersonAvailability Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonAvailability> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IPersonAvailability Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IPersonAvailability> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public ICollection<IPersonAvailability> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IPersonAvailability> LoadPersonAvailabilityWithHierarchyData(IEnumerable<IPerson> persons, DateOnly startDate)
		{
			//impl when needed
			return Enumerable.Empty<IPersonAvailability>();
		}
	}
}