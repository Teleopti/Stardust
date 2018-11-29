using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonAbsenceRepository : IPersonAbsenceRepository
	{
		private readonly IFakeStorage _storage;

		public FakePersonAbsenceRepository(IFakeStorage storage)
		{
			_storage = storage ?? new FakeStorageSimple();
		}

		public void Has(IPersonAbsence personAbsence)
		{
			Add(personAbsence);
		}

		public void Add(IPersonAbsence entity)
		{
			_storage.Add(entity);
		}

		public void Remove(IPersonAbsence entity)
		{
			_storage.Remove(entity);
		}

		public IPersonAbsence Get(Guid id)
		{
			return _storage.Get<IPersonAbsence>(id);
		}

		public IEnumerable<IPersonAbsence> LoadAll()
		{
			return _storage.LoadAll<IPersonAbsence>().ToList();
		}

		public IPersonAbsence Load(Guid id)
		{
			throw new NotImplementedException();
		}

		IPersonAbsence ILoadAggregateByTypedId<IPersonAbsence, Guid>.LoadAggregate(Guid id)
		{
			return LoadAggregate(id);
		}

		public IPersonAbsence LoadAggregate(Guid id)
		{
			return _storage.LoadAll<IPersonAbsence>().Single(pa => pa.Id == id);
		}

		public ICollection<IPersonAbsence> Find(IEnumerable<IPerson> persons, DateTimePeriod period)
		{
			return _storage.LoadAll<IPersonAbsence>().ToList();
		}

		public ICollection<IPersonAbsence> Find(IEnumerable<IPerson> persons, DateTimePeriod period, IScenario scenario)
		{
			return _storage.LoadAll<IPersonAbsence>()
					.Where(personAbsence => personAbsence.BelongsToScenario(scenario) && 
							personAbsence.Period.Intersect(period) &&
							persons.Contains(personAbsence.Person)).ToArray(); 
		}

		public ICollection<IPersonAbsence> Find(DateTimePeriod period, IScenario scenario)
		{
			return _storage.LoadAll<IPersonAbsence>().Where(x => x.BelongsToScenario(scenario) && x.Period.Intersect(period)).ToArray();
		}

		public ICollection<IPersonAbsence> Find(IEnumerable<Guid> personAbsenceIds, IScenario scenario)
		{
			var idList = personAbsenceIds.ToList();
			return _storage.LoadAll<IPersonAbsence>().Where(x => idList.Contains(x.Id.GetValueOrDefault())).ToArray();
		}

		public ICollection<DateTimePeriod> AffectedPeriods(IPerson person, IScenario scenario, DateTimePeriod period, IAbsence absence)
		{
			return _storage.LoadAll<IPersonAbsence>()
				.Where(personAbsence => personAbsence.Person == person && personAbsence.Period.Intersect(period) &&
										(absence == null || personAbsence.Layer.Payload == absence) && personAbsence.Scenario == scenario).Select(p => p.Period).ToArray();
		}

		
		public ICollection<IPersonAbsence> FindExact(IPerson person, DateTimePeriod period, IAbsence absence, IScenario scenario)
		{
			return _storage.LoadAll<IPersonAbsence>()
				.Where(personAbsence => personAbsence.Person == person && personAbsence.Period == period && personAbsence.Layer.Payload == absence && personAbsence.Scenario == scenario
			).ToList();
		}

		public bool IsThereScheduledAgents(Guid businessUnitId, DateOnlyPeriod period)
		{
			return _storage.LoadAll<IPersonAbsence>()
				.Any(pa =>
				{
					var inThisBu = pa.Person.PersonPeriodCollection.First().Team.Site.BusinessUnit.Id == businessUnitId;
					var startDate = new DateOnly(pa.Period.StartDateTime);
					var endDate = new DateOnly(pa.Period.EndDateTime);
					return inThisBu && (period.Contains(startDate) || period.Contains(endDate));
				});
		}
	}
}
