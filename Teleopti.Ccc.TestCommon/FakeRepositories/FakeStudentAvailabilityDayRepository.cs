using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

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
			Storage.Remove(root);
		}

		public IStudentAvailabilityDay Get(Guid id)
		{
			return Storage.FirstOrDefault(x => x.Id == id);
		}

		public IList<IStudentAvailabilityDay> LoadAll()
		{
			return Storage;
		}

		public IStudentAvailabilityDay Load(Guid id)
		{
			throw new NotImplementedException();
		}

		IStudentAvailabilityDay ILoadAggregateByTypedId<IStudentAvailabilityDay, Guid>.LoadAggregate(Guid id)
		{
			return LoadAggregate(id);
		}

		public IStudentAvailabilityDay LoadAggregate(Guid id)
		{
			return Storage.First(x => x.Id == id);
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