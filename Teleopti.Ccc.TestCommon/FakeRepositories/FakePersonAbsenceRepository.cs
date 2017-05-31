using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonAbsenceRepositoryLegacy : FakePersonAbsenceRepository
	{
		public FakePersonAbsenceRepositoryLegacy() : base(new FakeStorage())
		{
		}
	}

	public class FakePersonAbsenceRepository : IPersonAbsenceRepository
	{
		private readonly FakeStorage _storage;

		public FakePersonAbsenceRepository(FakeStorage storage)
		{
			_storage = storage;
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

		public IList<IPersonAbsence> LoadAll()
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
			throw new NotImplementedException();
		}

		
		public ICollection<IPersonAbsence> FindExact(IPerson person, DateTimePeriod period, IAbsence absence, IScenario scenario)
		{
			return _storage.LoadAll<IPersonAbsence>()
				.Where(personAbsence => personAbsence.Person == person && personAbsence.Period == period && personAbsence.Layer.Payload == absence && personAbsence.Scenario == scenario
			).ToList();
		}
	}
}
