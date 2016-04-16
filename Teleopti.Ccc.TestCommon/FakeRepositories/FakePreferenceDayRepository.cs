using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePreferenceDayRepository : IPreferenceDayRepository
	{
		private IList<IPreferenceDay> _preferensDays = new List<IPreferenceDay>();

		public void Add(IPreferenceDay root)
		{
			_preferensDays.Add(root);
		}

		public void Remove(IPreferenceDay root)
		{
			_preferensDays.Remove(root);
		}

		public IPreferenceDay Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IPreferenceDay> LoadAll()
		{
			return _preferensDays;
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
			return _preferensDays;
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