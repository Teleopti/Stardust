using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeOvertimeAvailabilityRepository : IOvertimeAvailabilityRepository
	{
		private readonly IList<IOvertimeAvailability> _overtimeAvailabilities = new List<IOvertimeAvailability>();
		public void Add(IOvertimeAvailability root)
		{
			_overtimeAvailabilities.Add(root);
		}

		public void Remove(IOvertimeAvailability root)
		{
			_overtimeAvailabilities.Remove(root);
		}

		public IOvertimeAvailability Get(Guid id)
		{
			return _overtimeAvailabilities.FirstOrDefault(x => x.Id == id);
		}

		public IList<IOvertimeAvailability> LoadAll()
		{
			return _overtimeAvailabilities;
		}

		public IOvertimeAvailability Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
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
			return _overtimeAvailabilities.First(x => x.Id == id);
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