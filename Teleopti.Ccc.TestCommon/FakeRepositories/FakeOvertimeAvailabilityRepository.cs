using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeOvertimeAvailabilityRepository : IOvertimeAvailabilityRepository
	{
		public void Add(IOvertimeAvailability root)
		{
			throw new NotImplementedException();
		}

		public void Remove(IOvertimeAvailability root)
		{
			throw new NotImplementedException();
		}

		public IOvertimeAvailability Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IOvertimeAvailability> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IOvertimeAvailability Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IOvertimeAvailability> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		IOvertimeAvailability ILoadAggregateByTypedId<IOvertimeAvailability, Guid>.LoadAggregate(Guid id)
		{
			return LoadAggregate(id);
		}

		public IOvertimeAvailability LoadAggregate(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IOvertimeAvailability> Find(DateOnlyPeriod period, IEnumerable<IPerson> persons)
		{
			//impl when needed
			return new List<IOvertimeAvailability>();
		}

		public IList<IOvertimeAvailability> Find(DateOnly dateOnly, IPerson person)
		{
			throw new NotImplementedException();
		}
	}
}