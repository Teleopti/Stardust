using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeStudentAvailabilityDayRepository : IStudentAvailabilityDayRepository
	{
		private readonly List<IStudentAvailabilityDay> Storage = new List<IStudentAvailabilityDay>();

		public void Add(IStudentAvailabilityDay root)
		{
			Storage.Add(root);
		}

		public void Remove(IStudentAvailabilityDay root)
		{
			throw new NotImplementedException();
		}

		public IStudentAvailabilityDay Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IStudentAvailabilityDay> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IStudentAvailabilityDay Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		IStudentAvailabilityDay ILoadAggregateByTypedId<IStudentAvailabilityDay, Guid>.LoadAggregate(Guid id)
		{
			return LoadAggregate(id);
		}

		public IStudentAvailabilityDay LoadAggregate(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IStudentAvailabilityDay> Find(DateOnlyPeriod period, IEnumerable<IPerson> persons)
		{
			//impl when needed
			return new List<IStudentAvailabilityDay>();
		}

		public IList<IStudentAvailabilityDay> Find(DateOnly dateOnly, IPerson person)
		{
			return Storage.Where(x => x.RestrictionDate == dateOnly && x.Person.Id == person.Id).ToList();
		}

		public IList<IStudentAvailabilityDay> FindNewerThan(DateTime newerThan)
		{
			throw new NotImplementedException();
		}
	}
}