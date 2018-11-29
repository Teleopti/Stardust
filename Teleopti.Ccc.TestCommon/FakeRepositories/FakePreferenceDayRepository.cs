using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePreferenceDayRepository : IPreferenceDayRepository
	{
		private readonly IList<IPreferenceDay> _preferensDays = new List<IPreferenceDay>();

		public void Add(IPreferenceDay root)
		{
			_preferensDays.Add(root);
		}

		public void Has(IPerson agent, DateOnly date, IPreferenceRestriction preferenceRestriction)
		{
			_preferensDays.Add(new PreferenceDay(agent, date, preferenceRestriction).WithId());
		}

		public void Has(IPerson agent, DateOnlyPeriod period, IPreferenceRestriction preferenceRestriction)
		{
			foreach (var date in period.DayCollection())
			{
				Has(agent,date,preferenceRestriction);	
			}
		}

		public void Remove(IPreferenceDay root)
		{
			_preferensDays.Remove(root);
		}

		public IPreferenceDay Get(Guid id)
		{
			return _preferensDays.FirstOrDefault(x => x.Id == id);
		}

		public IEnumerable<IPreferenceDay> LoadAll()
		{
			return _preferensDays;
		}

		public IPreferenceDay Load(Guid id)
		{
			throw new NotImplementedException();
		}

		IPreferenceDay ILoadAggregateByTypedId<IPreferenceDay, Guid>.LoadAggregate(Guid id)
		{
			return LoadAggregate(id);
		}

		public IPreferenceDay LoadAggregate(Guid id)
		{
			return _preferensDays.First(x => x.Id == id);
		}

		public IList<IPreferenceDay> Find(DateOnlyPeriod period, IEnumerable<IPerson> persons)
		{
			return _preferensDays.Where(a => period.Contains(a.RestrictionDate) && persons.Contains(a.Person)).ToList();
		}

		public IList<IPreferenceDay> Find(DateOnly dateOnly, IPerson person)
		{
			return _preferensDays.Where(a => a.RestrictionDate == dateOnly && a.Person == person).ToList();
		}

		public IList<IPreferenceDay> FindAndLock(DateOnly dateOnly, IPerson person)
		{
			throw new NotImplementedException();
		}

		public IList<IPreferenceDay> Find(DateOnlyPeriod period, IPerson person)
		{
			return _preferensDays.Where(a => period.Contains(a.RestrictionDate) && a.Person == person).ToList();
		}

		public IList<IPreferenceDay> FindNewerThan(DateTime newerThan)
		{
			throw new NotImplementedException();
		}

		public IPreferenceDay Find(Guid preferenceId)
		{
			return _preferensDays.Single(a => a.Id == preferenceId);
		}
	}
}