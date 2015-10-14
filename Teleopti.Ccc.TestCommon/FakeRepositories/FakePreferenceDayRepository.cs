using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePreferenceDayRepository : IPreferenceDayRepository
	{
		public void Add(IPreferenceDay root)
		{
			throw new NotImplementedException();
		}

		public void Remove(IPreferenceDay root)
		{
			throw new NotImplementedException();
		}

		public IPreferenceDay Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IPreferenceDay> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IPreferenceDay Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IPreferenceDay> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		IPreferenceDay ILoadAggregateByTypedId<IPreferenceDay, Guid>.LoadAggregate(Guid id)
		{
			return LoadAggregate(id);
		}

		public IPreferenceDay LoadAggregate(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IPreferenceDay> Find(DateOnlyPeriod period, IEnumerable<IPerson> persons)
		{
			//impl when needed
			return new List<IPreferenceDay>();
		}

		public IList<IPreferenceDay> Find(DateOnly dateOnly, IPerson person)
		{
			throw new NotImplementedException();
		}

		public IList<IPreferenceDay> FindAndLock(DateOnly dateOnly, IPerson person)
		{
			throw new NotImplementedException();
		}

		public IList<IPreferenceDay> Find(DateOnlyPeriod period, IPerson person)
		{
			throw new NotImplementedException();
		}

		public IList<IPreferenceDay> FindNewerThan(DateTime newerThan)
		{
			throw new NotImplementedException();
		}
	}
}